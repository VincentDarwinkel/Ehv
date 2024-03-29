﻿using Authentication_Service.CustomExceptions;
using Authentication_Service.Dal.Interface;
using Authentication_Service.Enums;
using Authentication_Service.Models.Dto;
using Authentication_Service.Models.HelperFiles;
using Authentication_Service.Models.ToFrontend;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Authentication_Service.Logic
{
    public class JwtLogic
    {
        private readonly IRefreshTokenDal _refreshTokenDal;
        private readonly JwtConfig _config;
        private readonly JsonWebTokenHandler _handler = new JsonWebTokenHandler();

        public JwtLogic(IRefreshTokenDal refreshTokenDal, JwtConfig config)
        {
            _refreshTokenDal = refreshTokenDal;
            _config = config;
        }

        /// <summary>
        /// Gets the security token descriptor with user claims
        /// </summary>
        /// <param name="user">The user to create the descriptor for</param>
        /// <returns>A security token descriptor</returns>
        private SecurityTokenDescriptor GetTokenDescriptor(UserDto user)
        {
            string secretKey = _config.Secret;
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new NoNullAllowedException();
            }

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var expirationDate = DateTime.UtcNow.AddMinutes(15);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = "Auth",
                Audience = _config.FrontendUrl,
                IssuedAt = DateTime.UtcNow,
                NotBefore = DateTime.UtcNow,
                Expires = expirationDate,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha512)
            };

            if (user != null) tokenDescriptor.Subject = new ClaimsIdentity(GetClaims(user));
            return tokenDescriptor;
        }
        private static IEnumerable<Claim> GetClaims(UserDto user)
        {
            return new List<Claim>
            {
                new Claim("Uuid", user.Uuid.ToString()),
                new Claim("AccountRole", user.AccountRole.ToString())
            };
        }

        /// <summary>
        /// Finds the claim in the expired jwt, allowed types are Guid, AccountRole and string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jwt"></param>
        /// <param name="claim"></param>
        /// <returns>The requested claim in the specified extension</returns>
        public T GetClaim<T>(string jwt, JwtClaim claim)
        {
            if (jwt == null)
            {
                throw new UnprocessableException();
            }

            string key = Enum.GetName(typeof(JwtClaim), claim);
            var handler = new JsonWebTokenHandler();
            JsonWebToken jwtToken = handler.ReadJsonWebToken(jwt);

            string foundClaim = jwtToken.Claims?
                .FirstOrDefault(c => c.Type.Equals(key, StringComparison.OrdinalIgnoreCase))?
                .Value;

            if (string.IsNullOrEmpty(foundClaim))
            {
                return default;
            }
            if (typeof(T) == typeof(Guid))
            {
                return (T)Convert.ChangeType(Guid.Parse(foundClaim), typeof(T), CultureInfo.InvariantCulture);
            }
            if (typeof(T) == typeof(AccountRole))
            {
                return (T)Convert.ChangeType(Enum.Parse<AccountRole>(foundClaim), typeof(T), CultureInfo.InvariantCulture);
            }

            return default;
        }

        /// <summary>
        /// Validates the expiredJwt
        /// </summary>
        /// <param name="jwt">The expired jwt too validate</param>
        /// <param name="jwtIsExpired">Set this to validate the expired jwt if it is expired, default false</param>
        /// <returns>The validation result</returns>
        public TokenValidationResult ValidateJwt(string jwt, bool jwtIsExpired = false)
        {
            SecurityTokenDescriptor tokenDescriptor = GetTokenDescriptor(null);
            if (string.IsNullOrEmpty(tokenDescriptor.Audience) || string.IsNullOrEmpty(jwt))
            {
                throw new ArgumentNullException(nameof(jwt));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config.Secret));
            return _handler.ValidateToken(jwt,
                new TokenValidationParameters
                {
                    ValidIssuer = tokenDescriptor.Issuer,
                    ValidAudience = tokenDescriptor.Audience,
                    IssuerSigningKey = signingKey,
                    RequireExpirationTime = true,
                    ValidateLifetime = !jwtIsExpired,

                    ValidateIssuerSigningKey = true,
                    ClockSkew = new TimeSpan(),
                });
        }

        /// <summary>
        /// Creates a jwt with the claims of the specified user
        /// </summary>
        /// <param name="user">The db user</param>
        /// <returns>An jwt and refresh token object</returns>
        public async Task<AuthorizationTokensViewmodel> CreateJwt(UserDto user, RefreshTokenDto oldRefreshToken = null)
        {
            if (user == null || user.Uuid == Guid.Empty || user.AccountRole == AccountRole.Undefined)
            {
                throw new ArgumentNullException(nameof(user));
            }

            string jwt = _handler.CreateToken(GetTokenDescriptor(user));
            await _refreshTokenDal.DeleteOutdatedTokens();

            var refreshTokenDto = new RefreshTokenDto
            {
                RefreshToken = Guid.NewGuid(),
                ExpirationDate = DateTime.Now.AddDays(7),
                UserUuid = user.Uuid
            };

            if (await _refreshTokenDal.Find(refreshTokenDto) != null)
            {
                await _refreshTokenDal.Delete(user.Uuid);
            }

            await _refreshTokenDal.Add(refreshTokenDto);
            return new AuthorizationTokensViewmodel
            {
                Jwt = jwt,
                RefreshToken = refreshTokenDto.RefreshToken
            };
        }

        /// <summary>
        /// Refreshes the expiredJwt and refresh token
        /// </summary>
        /// <param name="expiredJwt">The expired jwt too refresh</param>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="requestingUser">The user that made the request</param>
        /// <returns>The refreshed jwt and refresh token</returns>
        public async Task<AuthorizationTokensViewmodel> RefreshJwt(string expiredJwt, Guid refreshToken, UserDto requestingUser)
        {
            TokenValidationResult validationResult = ValidateJwt(expiredJwt, true);
            if (!validationResult.IsValid)
            {
                throw new UnauthorizedAccessException("The expired jwt is not valid");
            }

            RefreshTokenDto savedRefreshToken = _refreshTokenDal.Find(new RefreshTokenDto
            {
                RefreshToken = refreshToken,
                UserUuid = requestingUser.Uuid
            }).Result;

            if (refreshToken == Guid.Empty || savedRefreshToken?.RefreshToken != refreshToken)
            {
                throw new SecurityTokenException("Invalid refresh token");
            }

            if (savedRefreshToken.ExpirationDate < DateTime.Now)
            {
                await _refreshTokenDal.Delete(requestingUser.Uuid);
                throw new SecurityTokenException("Refresh token is expired");
            }

            return await CreateJwt(requestingUser, savedRefreshToken);
        }
    }
}
