﻿using File_Service.Models.HelperFiles;
using Microsoft.Extensions.Configuration;
using nClam;
using System.Threading.Tasks;

namespace File_Service.Logic
{
    public class VirusScannerLogic
    {
        private readonly IConfiguration _config;

        public VirusScannerLogic(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Sends the specified bytes to the ClamAv server to be scanned for viruses
        /// </summary>
        /// <param name="fileBytes">The file bytes to scanned</param>
        /// <returns>True if a virus is detected false if file is clean</returns>
        public async Task<bool> FileContainsVirus(byte[] fileBytes)
        {
            var clam = new ClamClient(_config[ConfigParameters.ClamAvServerUrl]);
            ClamScanResult scanResult = await clam.SendAndScanFileAsync(fileBytes);
            return scanResult.Result == ClamScanResults.VirusDetected;
        }
    }
}
