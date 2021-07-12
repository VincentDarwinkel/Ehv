﻿using File_Service.CustomExceptions;
using File_Service.Dal.Interfaces;
using File_Service.Enums;
using File_Service.Models;
using File_Service.Models.HelperFiles;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace File_Service.Logic
{
    public class FileLogic
    {
        private readonly FileHelper _fileHelper;
        private readonly DirectoryLogic _directoryLogic;
        private readonly IFileDal _fileDal;

        public FileLogic(FileHelper fileHelper, DirectoryLogic directoryLogic, IFileDal fileDal)
        {
            _fileHelper = fileHelper;
            _directoryLogic = directoryLogic;
            _fileDal = fileDal;
        }

        /// <summary>
        /// Saves the file on the file system if the provided userSpecifiedPath is valid, the file is an webp image or mp4 video and the file does not contain viruses
        /// </summary>
        /// <param name="files">The files to save</param>
        /// <param name="userSpecifiedPath">The userSpecifiedPath to save the files in</param>
        /// <param name="requestingUserUuid">The uuid of the requesting user</param>
        /// <returns>A list of the name of the files that are saved</returns>
        public async Task SaveFile(List<IFormFile> files, string userSpecifiedPath, Guid requestingUserUuid)
        {
            List<IFormFile> validFiles = await _fileHelper.FilterFiles(files);
            if (validFiles.Count == 0)
            {
                throw new UnprocessableException();
            }

            var fullPath = $"{Environment.CurrentDirectory}{userSpecifiedPath}";
            if (!Directory.Exists(fullPath))
            {
                await _directoryLogic.CreateDirectory(userSpecifiedPath, requestingUserUuid);
            }

            string[] supportedImageFileTypes = { ".webp", ".png", ".jpeg", ".jpg" };
            List<IFormFile> imageCollection = validFiles
                .FindAll(file => supportedImageFileTypes
                .Any(sift => file.FileName
                    .EndsWith(sift)));

            string[] supportedVideoFileTypes = { ".webm", ".mp4", ".mov", ".avi" };
            List<IFormFile> videoCollection = validFiles
                .FindAll(file => supportedVideoFileTypes
                    .Any(sift => file.FileName
                        .EndsWith(sift)));

            DirectoryDto parentDirectory = await _directoryLogic.Find(userSpecifiedPath);
            if (parentDirectory == null)
            {
                throw new NoNullAllowedException("parentDirectory was empty, no directory found with this paths in the database");
            }

            var filesToAdd = new List<FileDto>();
            foreach (var video in videoCollection)
            {
                string fileName = await CompressAndSaveVideo(video, fullPath);
                filesToAdd.Add(new FileDto
                {
                    Uuid = Guid.NewGuid(),
                    FileName = fileName,
                    FilePath = userSpecifiedPath,
                    FileType = FileType.Video,
                    OwnerUuid = requestingUserUuid,
                    ParentDirectoryUuid = parentDirectory.Uuid
                });
            }

            foreach (var image in imageCollection)
            {
                string fileName = await CompressAndSaveImage(image, fullPath);
                filesToAdd.Add(new FileDto
                {
                    Uuid = Guid.NewGuid(),
                    FileName = fileName,
                    FilePath = userSpecifiedPath,
                    FileType = FileType.Image,
                    OwnerUuid = requestingUserUuid,
                    ParentDirectoryUuid = parentDirectory.Uuid
                });
            }

            await _fileDal.Add(filesToAdd);
        }

        /// <summary>
        /// Compresses the image and saves the compressed image in the specified path
        /// </summary>
        /// <param name="image">The image to compress</param>
        /// <param name="path">The path to save the image to</param>
        /// <returns>The path of the compressed image</returns>
        private static async Task<string> CompressAndSaveImage(IFormFile image, string path)
        {
            string fileExtension = image.ContentType.Replace("image/", ".");
            var newFileName = Guid.NewGuid().ToString();
            var tempPath = $"{Environment.CurrentDirectory}/Media/TempFiles/{Guid.NewGuid()}/";
            Directory.CreateDirectory(tempPath);
            File.Copy($"{Environment.CurrentDirectory}/Media/TempFiles/ImageConverter.py", $"{tempPath}ImageConverter.py");
            await using (Stream fileStream = new FileStream($"{tempPath}input{fileExtension}", FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            SystemHelper.ExecuteOsCommand($"python3 {tempPath}ImageConverter.py");
            File.Move($"{tempPath}output.webp", $"{path}{newFileName}.webp");
            DirectoryHelper.DeleteDirectory(tempPath);

            return newFileName;
        }

        /// <summary>
        /// Compresses the video and saves the compressed video in the specified path
        /// </summary>
        /// <param name="video">The video to compress</param>
        /// <param name="path">The path to save the video to</param>
        /// <returns>The path of the compressed video</returns>
        private static async Task<string> CompressAndSaveVideo(IFormFile video, string path)
        {
            string fileExtension = video.ContentType.Replace("video/", ".");
            var tempFileName = Guid.NewGuid().ToString();
            var newFileName = Guid.NewGuid().ToString();
            var tempPath = $"{Environment.CurrentDirectory}/Media/TempFiles/";
            await using (Stream fileStream = new FileStream(tempPath + tempFileName + fileExtension, FileMode.Create))
            {
                await video.CopyToAsync(fileStream);
            }

            SystemHelper.ExecuteOsCommand($"ffmpeg -i {tempPath + tempFileName + fileExtension} -b:a 300k {path}{newFileName}.mp4");
            File.Delete(tempPath + tempFileName + fileExtension);
            return newFileName;
        }

        /// <summary>
        /// Removes a file by uuid if the user is owner and the file exists
        /// </summary>
        /// <param name="fileUuidCollection">The uuid of the file to remove</param>
        /// <param name="requestingUser">The user that made the request</param>
        public async Task Delete(List<Guid> fileUuidCollection, UserHelper requestingUser)
        {
            if (fileUuidCollection.Any(fu => fu == Guid.Empty))
            {
                throw new UnprocessableException();
            }

            List<FileDto> filesToDelete = await _fileDal.Find(fileUuidCollection);
            foreach (var file in filesToDelete)
            {
                string fullPath = $"{Environment.CurrentDirectory}{file.FilePath}";
                File.Delete(fullPath);
            }

            await _fileDal.Delete(filesToDelete);
        }
    }
}