﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using User_Service.CustomExceptions;
using User_Service.Dal.Interfaces;
using User_Service.Enums;
using User_Service.Models;
using User_Service.Models.FromFrontend;
using User_Service.Models.HelperFiles;
using User_Service.Models.RabbitMq;
using User_Service.RabbitMq.Publishers;
using User_Service.RabbitMq.Rpc;

namespace User_Service.Logic
{
    public class UserLogic
    {
        private readonly IUserDal _userDal;
        private readonly IDisabledUserDal _disabledUserDal;
        private readonly IActivationDal _activationDal;
        private readonly IMapper _mapper;
        private readonly IPublisher _publisher;
        private readonly IRpcClient _rpcClient;

        public UserLogic(IUserDal userDal, IDisabledUserDal disabledUserDal, IActivationDal activationDal,
            IMapper mapper, IPublisher publisher, IRpcClient rpcClient)
        {
            _userDal = userDal;
            _disabledUserDal = disabledUserDal;
            _activationDal = activationDal;
            _mapper = mapper;
            _publisher = publisher;
            _rpcClient = rpcClient;
        }

        private static bool EmailIsValidLocal(string email)
        {
            return new EmailAddressAttribute().IsValid(email);
        }

        private static async Task<bool> EmailIsValid(string email)
        {
            try
            {
                var url = new Uri("https://ehvemailvalidator.azurewebsites.net/api/emailvalidator?email=" + email);
                var httpClient = new HttpClient();
                var result = await httpClient.GetStringAsync(url);
                return Convert.ToBoolean(result);
            }
            catch (Exception)
            {
                return EmailIsValidLocal(email);
            }
        }

        private async Task<bool> UserModelValid(User user)
        {
            return !string.IsNullOrEmpty(user.Username) &&
                   user.Username.Length >= 4 &&
                   !string.IsNullOrEmpty(user.Email) &&
                   await EmailIsValid(user.Email);
        }

        /// <summary>
        /// Saves the user in the database
        /// </summary>
        /// <param name="user">The form data the user send</param>
        public async Task Register(User user)
        {
            if (!await UserModelValid(user) || user.Password.Length < 8)
            {
                throw new UnprocessableException();
            }

            bool usernameOrEmailInUse = await _userDal.Exists(user.Username, user.Email);
            if (usernameOrEmailInUse)
            {
                throw new DuplicateNameException();
            }

            bool databaseContainsUsers = await _userDal.Any();
            var userDto = _mapper.Map<UserDto>(user);
            userDto.AccountRole = databaseContainsUsers ? AccountRole.User : AccountRole.SiteAdmin;
            userDto.Uuid = Guid.NewGuid();

            var disabledUserDto = new DisabledUserDto
            {
                Reason = DisableReason.EmailVerificationRequired,
                UserUuid = userDto.Uuid,
                Uuid = Guid.NewGuid()
            };

            var activationDto = new ActivationDto
            {
                Code = Guid.NewGuid().ToString(),
                UserUuid = userDto.Uuid,
                Uuid = Guid.NewGuid()
            };

            await _disabledUserDal.Add(disabledUserDto);
            await _activationDal.Add(activationDto);

            var userRabbitMq = _mapper.Map<UserRabbitMqSensitiveInformation>(user);
            userRabbitMq.Uuid = userDto.Uuid;
            userRabbitMq.AccountRole = databaseContainsUsers ? AccountRole.User : AccountRole.SiteAdmin;

            _publisher.Publish(userRabbitMq, RabbitMqRouting.AddUser, RabbitMqExchange.AuthenticationExchange);

            await _userDal.Add(userDto);
            SendActivationEmail(userDto, activationDto);
        }

        private void SendActivationEmail(UserDto user, ActivationDto activation)
        {
            var email = new EmailRabbitMq
            {
                EmailAddress = user.Email,
                TemplateName = "ActivateAccount",
                Subject = "Activatie Eindhovense vriendjes",
                KeyWordValues = new List<EmailKeyWordValue>
                {
                    new EmailKeyWordValue
                    {
                        Key = "Username",
                        Value = user.Username
                    },
                    new EmailKeyWordValue
                    {
                        Key = "ActivationCode",
                        Value = activation.Code
                    }
                }
            };

            _publisher.Publish(new List<EmailRabbitMq> { email }, RabbitMqRouting.SendMail, RabbitMqExchange.MailExchange);
        }

        /// <returns>All users in the database</returns>
        public async Task<List<UserDto>> All()
        {
            return await _userDal.All();
        }

        /// <summary>
        /// Finds all users which match the uuid in the collection
        /// </summary>
        /// <param name="uuidCollection">The uuid collection</param>
        /// <returns>The found users, null if nothing is found</returns>
        public async Task<List<UserDto>> Find(List<Guid> uuidCollection)
        {
            return await _userDal.Find(uuidCollection);
        }

        /// <summary>
        /// Finds all users which match the uuid in the collection
        /// </summary>
        /// <param name="uuidCollectionJson">The uuid collection in json</param>
        /// <returns>The found users, null if nothing is found</returns>
        public async Task<string> Find(string uuidCollectionJson)
        {
            var uuidCollection = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Guid>>(uuidCollectionJson);
            List<UserDto> foundUsers = await _userDal.Find(uuidCollection);
            var users = _mapper.Map<List<UserRabbitMq>>(foundUsers ?? new List<UserDto>());
            return Newtonsoft.Json.JsonConvert.SerializeObject(users);
        }

        /// <summary>
        /// Finds the user by uuid
        /// </summary>
        /// <param name="uuid">The uuid to search for</param>
        /// <returns>The found user, null if nothing is found</returns>
        public async Task<UserDto> Find(Guid uuid)
        {
            return await _userDal.Find(uuid);
        }

        /// <summary>
        /// Updates the user
        /// </summary>
        /// <param name="user">The new user data</param>
        /// <param name="requestingUserUuid">the uuid of the user that made the request</param>
        public async Task Update(User user, Guid requestingUserUuid)
        {
            if (!await UserModelValid(user))
            {
                throw new UnprocessableException();
            }

            UserDto dbUser = await _userDal.Find(requestingUserUuid);
            var userRabbitMq = _mapper.Map<UserRabbitMqSensitiveInformation>(user);

            userRabbitMq.Uuid = dbUser.Uuid;
            userRabbitMq.AccountRole = dbUser.AccountRole;

            var passwordValid = _rpcClient.Call<bool>(userRabbitMq, RabbitMqQueues.ValidateUserPasswordQueue);
            if (!passwordValid)
            {
                throw new UnauthorizedAccessException();
            }

            await CheckForDuplicatedUserData(user, dbUser);

            dbUser.Username = user.Username;
            dbUser.Email = user.Email;
            dbUser.About = user.About;
            dbUser.ReceiveEmail = user.ReceiveEmail;
            dbUser.Hobbies = _mapper.Map<List<UserHobbyDto>>(user.Hobbies);
            dbUser.FavoriteArtists = _mapper.Map<List<FavoriteArtistDto>>(user.FavoriteArtists);

            if (!string.IsNullOrEmpty(user.NewPassword) || dbUser.Username != user.Username)
            {
                userRabbitMq.Password = string.IsNullOrEmpty(user.NewPassword) ? user.Password : user.NewPassword;
                _publisher.Publish(userRabbitMq, RabbitMqRouting.UpdateUser, RabbitMqExchange.UserExchange);
            }

            await _userDal.Update(dbUser);
        }

        /// <summary>
        /// Checks if the username and email is already in use
        /// </summary>
        /// <param name="user">The updated user data</param>
        /// <param name="dbUser">The found user in the database by uuid</param>
        private async Task CheckForDuplicatedUserData(User user, UserDto dbUser)
        {
            if (user.Email != dbUser.Email && await _userDal.Exists(null, user.Email))
            {
                throw new DuplicateNameException();
            }

            if (user.Username != dbUser.Username && await _userDal.Exists(user.Username, null))
            {
                throw new DuplicateNameException();
            }
        }

        /// <summary>
        /// Deletes the user by uuid
        /// </summary>
        /// <param name="requestingUser">The user that made the request</param>
        /// <param name="userUuidToDeleteUuid">The uuid of the user to remove</param>
        public async Task Delete(UserDto requestingUser, Guid userUuidToDeleteUuid)
        {
            UserDto dbUserToDelete = await _userDal.Find(userUuidToDeleteUuid);
            if (dbUserToDelete == null)
            {
                throw new KeyNotFoundException();
            }

            switch (requestingUser.AccountRole)
            {
                case AccountRole.SiteAdmin:
                    int totalSiteAdmins = await _userDal.Count(AccountRole.SiteAdmin);
                    if (totalSiteAdmins <= 1)
                    {
                        throw new SiteAdminRequiredException();
                    }
                    _publisher.Publish(userUuidToDeleteUuid, RabbitMqRouting.DeleteUser, RabbitMqExchange.UserExchange);
                    await _userDal.Delete(userUuidToDeleteUuid);
                    break;
                case AccountRole.Admin when dbUserToDelete.AccountRole == AccountRole.User:
                    _publisher.Publish(userUuidToDeleteUuid, RabbitMqRouting.DeleteUser, RabbitMqExchange.UserExchange);
                    await _userDal.Delete(userUuidToDeleteUuid);
                    break;
                case AccountRole.User when requestingUser.Uuid == userUuidToDeleteUuid:
                    _publisher.Publish(userUuidToDeleteUuid, RabbitMqRouting.DeleteUser, RabbitMqExchange.UserExchange);
                    await _userDal.Delete(userUuidToDeleteUuid);
                    break;
                case AccountRole.Undefined:
                    break;
                default:
                    throw new UnauthorizedAccessException();
            }
        }

        public async Task Delete(Guid userUuid)
        {
            await _userDal.Delete(userUuid);
        }
    }
}
