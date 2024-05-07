using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Accounts;
using Melbeez.Data.Identity;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains CRUD operation of User and Authentication
    /// </summary>
    [Route("api/user")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class UserController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUserRefreshTokenManager _userRefreshTokenManager;
        private readonly IEmailManager _emailManager;
        private readonly ISMSManager _smsManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOtpManager _otpManager;
        private readonly IAdminTransactionLogManager _adminTransactionLogManager;
        private readonly IUserNotificationPreferenceManager _userNotificationPreferenceManager;
        private readonly ILocationsManager _locationsManager;
        private readonly IItemTransferInvitationManager _itemTransferInvitationManager;
        public UserController(IConfiguration configuration
                            , UserManager<ApplicationUser> userManager
                            , SignInManager<ApplicationUser> signInManager
                            , IUserRefreshTokenManager userRefreshTokenManager
                            , IEmailManager emailManager
                            , IUnitOfWork unitOfWork
                            , ISMSManager smsManager
                            , IOtpManager otpManager
                            , IAdminTransactionLogManager adminTransactionLogManager
                            , IUserNotificationPreferenceManager userNotificationPreferenceManager
                            , ILocationsManager locationsManager
                            , IItemTransferInvitationManager itemTransferInvitationManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _userRefreshTokenManager = userRefreshTokenManager;
            _emailManager = emailManager;
            _unitOfWork = unitOfWork;
            _smsManager = smsManager;
            _otpManager = otpManager;
            _adminTransactionLogManager = adminTransactionLogManager;
            _userNotificationPreferenceManager = userNotificationPreferenceManager;
            _locationsManager = locationsManager;
            _itemTransferInvitationManager = itemTransferInvitationManager;
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
            if (model.UserName.Contains(" "))
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Username cannot contain white spaces."
                });
            }

            if (!string.IsNullOrEmpty(model.UserName))
            {
                var userName = _userManager.Users.Where(x => !x.IsDeleted && x.IsUser && x.UserName.ToUpper() == model.UserName.ToUpper()).FirstOrDefault();
                if (userName != null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = "Username already exists."
                    });
                }
            }
            if (!string.IsNullOrEmpty(model.Email))
            {
                var email = _userManager.Users.Where(x => !x.IsDeleted && x.IsUser && x.Email.ToUpper() == model.Email.ToUpper()).FirstOrDefault();
                if (email != null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = "Email already exists."
                    });
                }
            }
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var phoneNumber = _userManager.Users.Where(x => !x.IsDeleted && x.IsUser && x.PhoneNumber == model.PhoneNumber).FirstOrDefault();
                if (phoneNumber != null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = "Phone number already exists."
                    });
                }
            }

            var user = new ApplicationUser()
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                CountryCode = model.CountryCode,
                PhoneNumber = model.PhoneNumber,
                CreatedDate = DateTime.UtcNow,
                IsFirstLoginAttempt = true,
                VerificationRemindedOn = DateTime.UtcNow,
                IsUser = true
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRole.User.ToString());
                await SendEmailVerificationLink(user.Email, true);
                await SendPhoneVerificationLink(user.CountryCode, user.PhoneNumber, true);

                #region Set User Notification Preference Bydefault true

                await _userNotificationPreferenceManager.AddUserNotificationPreference(new UserNotificationPreferenceResponseModel()
                {
                    IsWarrantyExpireAlert = true,
                    IsProductUpdateAlert = true,
                    IsLocationUpdateAlert = true,
                    IsDeviceActivationAlert = false,
                    IsMarketingValueAlert = false,
                    IsPushNotification = true,
                    IsEmailNotification = false,
                    IsTextNotification = false,
                    IsThirdPartyServiceAllowed = false,
                    IsBiometricAllowed = false
                }, user.Id);

                #endregion

                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = true,
                    Result = true,
                    Message = "Account has been created successfully.",
                });
            }
            else
            {
                var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = message
                });
            }
        }

        /// <summary>
        /// Authenticate a user with username and password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] LoginModel model)
        {
            var user = new ApplicationUser();
            bool isUser = true;
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }
            if (isUser)
            {
                user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted && x.IsUser
                                && (x.Email.ToLower() == model.Username.ToLower()
                                    || x.PhoneNumber == model.Username
                                    || x.UserName.ToLower() == model.Username.ToLower()
                                   ))
                       .FirstOrDefault();
            }
            else
            {

                user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted && !x.IsUser
                                && (x.Email.ToLower() == model.Username.ToLower()
                                    || x.PhoneNumber == model.Username
                                    || x.UserName.ToLower() == string.Concat(model.Username, "_Portal").ToLower()
                                   ))
                       .FirstOrDefault();
            }
            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<string>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = "The username or password doesnt match our records",
                    StatusCode = 401
                });
            }
            var _IsValid = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, false, lockoutOnFailure: true);
            if (_IsValid.Succeeded)
            {
                await _signInManager.UserManager.ResetAccessFailedCountAsync(user);
                await _signInManager.UserManager.SetLockoutEndDateAsync(user, null);
                return ResponseResult(new ManagerBaseResponse<AuthenticateModel>()
                {
                    IsSuccess = true,
                    Result = await GetAuthTokenResponse(user),
                });
            }
            if (user.LockoutEnd != null && user.AccessFailedCount > 2)
            {
                user.IsPermanentLockOut = true;
                await _userManager.UpdateAsync(user);
                await _signInManager.UserManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(99));
                return ResponseResult(new ManagerBaseResponse<string>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = "Your account has been permanently locked. Please contact administrator for unlock your account.",
                    StatusCode = 401
                });
            }
            if (_IsValid.IsLockedOut)
            {
                return ResponseResult(new ManagerBaseResponse<string>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = "Your account has been temporarily locked for 15 minutes.",
                    StatusCode = 401
                });
            }
            else
            {
                return ResponseResult(new ManagerBaseResponse<string>()
                {
                    IsSuccess = false,
                    Result = null,
                    Message = "The username or password doesnt match our records",
                    StatusCode = 401
                });
            }
        }

        /// <summary>
        /// Check user is exist or not 
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("isuserexist")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public IActionResult IsUserExist(string username)
        {
            bool isUser = true;
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
                username = string.Concat(username, "_Portal");
            }
            var user = _userManager
                       .Users
                       .Any(x => !x.IsDeleted
                              && x.UserName.ToLower() == username.ToLower()
                              && x.IsUser == isUser
                           );

            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Result = user,
                Message = user ? "User already exist." : "User does not exist.",
            });
        }

        /// <summary>
        /// Create or authenticate a user via external source
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpPost("external-login")]
        public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "Requested model is not valid.",
                    StatusCode = 500
                });
            }

            bool isUser = true;
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }

            var user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.IsUser == isUser
                                && x.Email.ToUpper() == model.Email.ToUpper()
                             )
                       .FirstOrDefault();

            if (user != null)
            {
                var notificationPreferences = await _userNotificationPreferenceManager.Get(user.Id);
                if (notificationPreferences.Result == null)
                {
                    await _userNotificationPreferenceManager.AddUserNotificationPreference(new UserNotificationPreferenceResponseModel()
                    {
                        IsWarrantyExpireAlert = true,
                        IsProductUpdateAlert = true,
                        IsLocationUpdateAlert = true,
                        IsDeviceActivationAlert = false,
                        IsMarketingValueAlert = false,
                        IsPushNotification = true,
                        IsEmailNotification = false,
                        IsTextNotification = false,
                        IsThirdPartyServiceAllowed = false,
                        IsBiometricAllowed = false
                    }, user.Id);
                }

                var result = await _signInManager.ExternalLoginSignInAsync(model.Provider, model.ProviderId, isPersistent: false, bypassTwoFactor: true);
                if (result.Succeeded)
                {
                    return ResponseResult(new ManagerBaseResponse<AuthenticateModel>()
                    {
                        IsSuccess = true,
                        Result = await GetAuthTokenResponse(user, true),
                    });
                }
                if (user.LockoutEnd != null && user.AccessFailedCount > 2)
                {
                    return ResponseResult(new ManagerBaseResponse<string>()
                    {
                        IsSuccess = false,
                        Result = null,
                        Message = "Your account has been permanently locked. Please contact administrator for unlock your account.",
                        StatusCode = 401
                    });
                }
                if (result.IsLockedOut)
                {
                    return ResponseResult(new ManagerBaseResponse<string>()
                    {
                        IsSuccess = false,
                        Result = null,
                        Message = "Your account has been temporarily locked for 15 minutes.",
                        StatusCode = 401
                    });
                }

                UserLoginInfo info = new UserLoginInfo(model.Provider, model.ProviderId, model.Provider);
                var resp = await _userManager.AddLoginAsync(user, info);
                return ResponseResult(new ManagerBaseResponse<AuthenticateModel>()
                {
                    IsSuccess = true,
                    Result = await GetAuthTokenResponse(user, true),
                });
            }
            else
            {
                if (!isUser)
                {
                    model.UserName = string.IsNullOrEmpty(model.UserName) ? string.Concat(model.Email, "_Portal") : string.Concat(model.UserName, "_Portal");
                }

                var applicationUser = new ApplicationUser()
                {
                    UserName = string.IsNullOrEmpty(model.UserName) ? model.Email : model.UserName,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ProfileUrl = model.ProfileUrl,
                    CreatedDate = DateTime.UtcNow,
                    IsFirstLoginAttempt = true,
                    VerificationRemindedOn = DateTime.UtcNow,
                    IsUser = true
                };

                var identityResult = await _userManager.CreateAsync(applicationUser);
                if (identityResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(applicationUser, UserRole.User.ToString());
                    UserLoginInfo info = new UserLoginInfo(model.Provider, model.ProviderId, model.Provider);
                    var resp = await _userManager.AddLoginAsync(applicationUser, info);
                    var User = _userManager.Users.Where(x => !x.IsDeleted && x.UserName.ToUpper() == applicationUser.UserName.ToUpper()).FirstOrDefault();

                    #region Set User Notification Preference Bydefault true

                    await _userNotificationPreferenceManager.AddUserNotificationPreference(new UserNotificationPreferenceResponseModel()
                    {
                        IsWarrantyExpireAlert = true,
                        IsProductUpdateAlert = true,
                        IsLocationUpdateAlert = true,
                        IsDeviceActivationAlert = true,
                        IsMarketingValueAlert = false,
                        IsPushNotification = true,
                        IsEmailNotification = true,
                        IsTextNotification = true,
                        IsThirdPartyServiceAllowed = false,
                        IsBiometricAllowed = false
                    }, User.Id);

                    #endregion

                    if (!string.IsNullOrEmpty(User.Email))
                    {
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(User);
                        await _userManager.ConfirmEmailAsync(User, token);
                    }
                    return ResponseResult(new ManagerBaseResponse<AuthenticateModel>()
                    {
                        IsSuccess = true,
                        Result = await GetAuthTokenResponse(User, true),
                        Message = "Account has been created successfully.",
                    });
                }
                else
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = identityResult.Errors.Select(s => s.Description).First()
                    });
                }
            }
        }

        /// <summary>
        /// Update an existing user's data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] UserUpdateModel model)
        {
            var user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.Id == User.Claims.GetUserId()
                             )
                       .FirstOrDefault();

            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "User not found.",
                    StatusCode = 500
                });
            }
            if (!string.IsNullOrEmpty(model.Email) && IsEmailAlreadyExist(model.Email, user.Id, user.IsUser))
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "Email already exists.",
                    StatusCode = 500
                });
            }
            if (!string.IsNullOrEmpty(model.PhoneNumber) && IsPhoneAlreadyExist(model.PhoneNumber, user.Id, user.IsUser))
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "Phone number already exists.",
                    StatusCode = 500
                });
            }
            if (!string.IsNullOrEmpty(model.UserName) && IsUserNameAlreadyExist(model.UserName, user.Id, user.IsUser))
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "User name already exists.",
                    StatusCode = 500
                });
            }

            var oldEmail = user.Email.ToLower();
            var oldPhoneNumber = string.Concat(model.CountryCode, model.PhoneNumber);

            if (!user.IsUser)
            {
                model.UserName = string.Concat(model.UserName, "_Portal");
            }

            user.FirstName = model.FirstName ?? user.FirstName;
            user.LastName = model.LastName ?? user.LastName;
            user.Email = model.Email ?? user.Email;
            user.CountryCode = model.CountryCode;
            user.PhoneNumber = model.PhoneNumber ?? user.PhoneNumber;
            user.UserName = model.UserName ?? user.UserName;
            user.ProfileUrl = model.ProfileUrl ?? user.ProfileUrl;
            user.CurrencyCode = model.CurrencyCode ?? user.CurrencyCode;
            user.UpdatedDate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(model.Email) && oldEmail != model.Email.ToLower())
            {
                user.EmailConfirmed = false;
                var userLogins = await _userManager.GetLoginsAsync(user);
                foreach (var item in userLogins)
                {
                    await _userManager.RemoveLoginAsync(user, item.LoginProvider, item.ProviderKey);
                }
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string emailVerificationUrl = _configuration["WebPages:BaseUrl"] + "user/email/verify";
                emailVerificationUrl = emailVerificationUrl + $"?Email={UtilityHelper.Base64Encode(user.Email)}&Code={UtilityHelper.Base64Encode(token)}&IsUser={user.IsUser}";
                await _emailManager.SetEmailUpdateEmail(user.Email, string.Concat(user.FirstName, " ", user.LastName), emailVerificationUrl, user.Id);
            }
            if (!string.IsNullOrEmpty(string.Concat(model.CountryCode, model.PhoneNumber)) && oldPhoneNumber != string.Concat(model.CountryCode, model.PhoneNumber))
            {
                user.PhoneNumberConfirmed = false;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User details has been updated successfully.",
                    Result = true
                });
            }
            else
            {
                var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = message,
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Get an authenticated users information
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<UserResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var user = _userManager.Users.Where(x => !x.IsDeleted && x.Id == User.Claims.GetUserId()).FirstOrDefault();
            var mediaBaseUrl = _configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var response = new UserResponseModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.UserName.Replace("_Portal", string.Empty),
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                CountyCode = user.CountryCode,
                CurrencyCode = user.CurrencyCode,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                ProfileUrl = !string.IsNullOrEmpty(user.ProfileUrl) ? mediaBaseUrl + user.ProfileUrl : user.ProfileUrl,
                IsTemporaryLockOut = user.LockoutEnd.HasValue ? (user.LockoutEnd.Value.DateTime > DateTime.UtcNow && !user.IsPermanentLockOut ? true : false) : false,
                IsPermanentLocuOut = user.IsPermanentLockOut,
                IsBlockedByAdmin = user.IsUserBlockedByAdmin,
                IsVerifiedByAdmin = user.IsVerifiedByAdmin,
                CreateOn = user.CreatedDate,
                UserAddresses = _unitOfWork.AddressesRepository
                    .GetQueryable(u =>!u.IsDeleted && u.CreatedBy == user.Id)
                    .Select(x => new UserAddressesModel()
                    {
                        AddressId = x.Id,
                        AddressLine1 = x.AddressLine1,
                        AddressLine2 = x.AddressLine2,
                        CityName = x.CityName,
                        ZipCode = x.ZipCode,
                        StateName = x.StateName,
                        CountryName = x.CountryName,
                        TypeOfProperty = x.TypeOfProperty,
                        IsDefault = x.IsDefault,
                        IsSameMailingAddress = x.IsSameMailingAddress
                    }).ToList(),
            };
            return ResponseResult(new ManagerBaseResponse<UserResponseModel>()
            {
                Result = response,
                IsSuccess = true,
            });
        }

        /// <summary>
        /// Change a password 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "Requested model is not valid.",
                    StatusCode = 500
                });
            }

            var user = _userManager.Users.Where(x => !x.IsDeleted && x.Id == User.Claims.GetUserId()).FirstOrDefault();
            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "User not found.",
                    StatusCode = 500
                });
            }
            var isValidOldPassword = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!isValidOldPassword)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "Please enter valid current password.",
                    StatusCode = 500
                });
            }
            if (model.OldPassword == model.NewPassword)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "Can't set your old password as new password.",
                    StatusCode = 500
                });
            }
            IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = true,
                    Message = "Your password has been changed successfully."
                });
            }
            else
            {
                var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = message,
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Get authentication token using token and refresh-token
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<AuthenticateModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
        {
            var key = _configuration.GetValue<string>("JWTSecretKey");
            ClaimsPrincipal principal;
            try
            {
                principal = UtilityHelper.GetPrincipalFromExpiredToken(key, model.Token);
            }
            catch (ArgumentException exception)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Message = "Invalid access token.",
                    Result = false,
                    StatusCode = 500
                });
            }
            var user = _userManager.Users.Where(x => !x.IsDeleted && x.Id == principal.GetJWTUserId()).FirstOrDefault();
            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Message = "Invalid access token.",
                    Result = false,
                    StatusCode = 500
                });
            }
            bool IsValid = _userRefreshTokenManager.IsValid(model.RefreshToken, user.Id);
            if (!IsValid)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Message = "Invalid access token.",
                    Result = false,
                    StatusCode = 400
                });
            }
            return ResponseResult(new ManagerBaseResponse<AuthenticateModel>()
            {
                IsSuccess = true,
                Result = await GetAuthTokenResponse(user)
            });
        }

        /// <summary>
        /// Reset a password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("reset-password")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword(ForgotPasswordVerifyRequest model)
        {
            bool isUser = true;
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }
            var user = new ApplicationUser();
            if (IsStringContainsEmail(model.Emailorphone))
            {
                user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.IsUser == isUser
                                && x.Email.ToUpper() == model.Emailorphone.ToUpper()
                             )
                       .FirstOrDefault();

                if (user == null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Message = "User not found.",
                        Result = false,
                        StatusCode = 404
                    });
                }
            }
            else
            {
                user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.IsUser == isUser
                                && x.PhoneNumber == model.Emailorphone
                             )
                       .FirstOrDefault();

                if (user == null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Message = "User not found.",
                        Result = false,
                        StatusCode = 404
                    });
                }
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (!resetPassResult.Succeeded)
            {
                var message = string.Join(" | ", resetPassResult.Errors.Select(x => x.Description));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = message,
                    Result = false,
                    StatusCode = 500
                });
            }
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Message = "Your password has been reset successfully.",
                Result = resetPassResult.Succeeded
            });
        }

        /// <summary>
        /// Send an email verification link
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="isUser"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("email/send-verification-link")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendEmailVerificationLink(string Email, bool isUser = true)
        {
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }
            var user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.IsUser == isUser
                                && x.Email.ToUpper() == Email.ToUpper()
                             )
                       .FirstOrDefault();

            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User not found.",
                    Result = false,
                    StatusCode = 500
                });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string EmailVerificationUrl = _configuration["WebPages:EmailVerificationUrl"] + $"?Email={UtilityHelper.Base64Encode(user.Email)}&Code={UtilityHelper.Base64Encode(token)}&IsUser={isUser}";
            var name = string.Concat(user.FirstName, " ", user.LastName);
            var response = await _emailManager.SetEmailVerificationLink(name, user.Email, EmailVerificationUrl, user.Id);
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Message = response.Message,
                Result = response.Result,
                StatusCode = response.StatusCode
            });
        }

        /// <summary>
        /// Verify an email
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Code"></param>
        /// <param name="IsUser"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("email/verify")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ConfirmEmail(string Email, string Code, bool IsUser)
        {
            var user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.IsUser == IsUser
                                && x.Email.ToUpper() == UtilityHelper.Base64Decode(Email).ToUpper()
                             )
                       .FirstOrDefault();

            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User not found.",
                    Result = false,
                    StatusCode = 500
                });
            }
            var result = await _userManager.ConfirmEmailAsync(user, UtilityHelper.Base64Decode(Code));
            if (!result.Succeeded)
            {
                var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = message,
                    Result = false,
                    StatusCode = 500
                });
            }
            else
            {
                await _itemTransferInvitationManager.InviteUserStatusUpdate(user);
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "Email verification has been done successfully.",
                    Result = true
                });
            }
        }

        /// <summary>
        /// Send an otp
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="emailorphone"></param>
        /// <param name="purpose"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("sendotp")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendOtp(string countryCode, string emailorphone, string purpose = null)
        {
            bool isUser = true;
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }
            if (!string.IsNullOrEmpty(emailorphone))
            {
                var user = new ApplicationUser();
                if (IsStringContainsEmail(emailorphone))
                {
                    user = _userManager.Users
                           .Where(x => !x.IsDeleted && x.IsUser == isUser
                                    && x.Email.ToUpper() == emailorphone.ToUpper())
                           .FirstOrDefault();

                    if (user != null)
                    {
                        var response = await _otpManager.AddOtp(null, emailorphone, user.Id);
                        ManagerBaseResponse<bool> mailSendResponse = new ManagerBaseResponse<bool>();
                        if (!string.IsNullOrEmpty(purpose) && purpose.ToLower() == "transferverification")
                        {
                            mailSendResponse = await _emailManager.SetItemTransferVerificationEmail(string.Concat(user.FirstName, " ", user.LastName), emailorphone, response, user.Id);
                        }
                        else
                        {
                            mailSendResponse = await _emailManager.SetOtpEmail(string.Concat(user.FirstName, " ", user.LastName), emailorphone, response, user.Id);
                        }
                        return ResponseResult(new ManagerBaseResponse<bool>()
                        {
                            Message = mailSendResponse.Message,
                            Result = mailSendResponse.Result,
                            StatusCode = mailSendResponse.StatusCode
                        });
                    }
                }
                else
                {
                    user = _userManager
                           .Users
                           .Where(x => !x.IsDeleted
                                    && x.IsUser == isUser
                                    && x.PhoneNumber == emailorphone
                                 )
                           .FirstOrDefault();

                    if (user != null)
                    {
                        var response = await _otpManager.AddOtp(emailorphone, null, user.Id);
                        ManagerBaseResponse<bool> smsSendResponse = new ManagerBaseResponse<bool>();
                        if (!string.IsNullOrEmpty(purpose) && purpose.ToLower() == "transferverification")
                        {
                            smsSendResponse = await _smsManager.SetItemTransferVerificationSms(string.Concat(countryCode, emailorphone), response, user.Id);
                        }
                        else
                        {
                            smsSendResponse = await _smsManager.SetOTPSms(string.Concat(countryCode, emailorphone), response, user.Id);
                        }
                        return ResponseResult(new ManagerBaseResponse<bool>()
                        {
                            Message = smsSendResponse.Message,
                            Result = smsSendResponse.Result,
                            StatusCode = smsSendResponse.StatusCode
                        });
                    }
                }
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User does not exist.",
                    Result = false,
                    StatusCode = 500
                });
            }
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Message = "Please enter the registered email/phone number",
                Result = false,
                StatusCode = 500
            });
        }

        /// <summary>
        /// Verify an otp
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("verifyotp")]
        public async Task<IActionResult> OTPVefiy(OTPVerifyModel model)
        {
            var user = new ApplicationUser();
            bool result = false;
            bool isUser = true;
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }
            if (!string.IsNullOrEmpty(model.Emailorphone))
            {
                if (IsStringContainsEmail(model.Emailorphone))
                {
                    user = _userManager
                           .Users
                           .Where(x => !x.IsDeleted
                                    && x.IsUser == isUser
                                    && x.Email.ToUpper() == model.Emailorphone.ToUpper()
                                 )
                           .FirstOrDefault();
                }
                else
                {
                    user = _userManager
                           .Users
                           .Where(x => !x.IsDeleted
                                    && x.IsUser == isUser
                                    && x.PhoneNumber == model.Emailorphone
                                 )
                           .FirstOrDefault();
                }

                if (user != null)
                {
                    result = await _otpManager.VerifyOtp(model.Otp, _configuration["OTPOverrideValue"].ToString(), user.Id);
                }

                #region Generate Password Reset Token

                if (result == true && model.purpose == "RESET_PASSWORD" && user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    return ResponseResult(new ManagerBaseResponse<dynamic>()
                    {
                        Message = "Otp verify successfully.",
                        Result = new ResetPasswordOTPRespnoseModel()
                        {
                            IsSuccess = true,
                            ResetPasswordToken = token
                        }
                    });
                }

                #endregion
            }
            var message = result == true ? "Otp verify successfully." : "Please provide valid otp.";
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Message = message,
                Result = result
            });
        }

        /// <summary>
        /// Recover username
        /// </summary>
        /// <param name="Emailorphone"></param>
        /// <param name="countryCode"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("recover-username")]
        public async Task<IActionResult> RecoverUsername(string countryCode, string Emailorphone)
        {
            bool isUser = true;
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }
            if (!string.IsNullOrEmpty(Emailorphone))
            {
                if (IsStringContainsEmail(Emailorphone))
                {
                    var user = _userManager
                               .Users
                               .Where(x => !x.IsDeleted
                                        && x.IsUser == isUser
                                        && x.Email.ToUpper() == Emailorphone.ToUpper()
                                     )
                               .FirstOrDefault();

                    if (user != null)
                    {
                        var mailResponse = await _emailManager
                                           .SetRecoverUserNameEmail(string.Concat(user.FirstName, " ", user.LastName)
                                                                    , user.Email
                                                                    , user.UserName.Replace(@"_Portal", string.Empty)
                                                                    , user.Id);

                        return ResponseResult(new ManagerBaseResponse<bool>()
                        {
                            Message = mailResponse.Message,
                            Result = mailResponse.Result,
                            StatusCode = mailResponse.StatusCode
                        });
                    }
                }
                else
                {
                    var user = _userManager
                               .Users
                               .Where(x => !x.IsDeleted
                                        && x.IsUser == isUser
                                        && x.PhoneNumber == Emailorphone
                                     )
                               .FirstOrDefault();
                    if (user != null)
                    {
                        var response = await _smsManager
                                       .SetRecoverUsernameSms(string.Concat(countryCode, Emailorphone)
                                                              , user.UserName.Replace(@"_Portal", string.Empty)
                                                              , User.Claims.GetUserId());

                        return ResponseResult(new ManagerBaseResponse<bool>()
                        {
                            Message = response.Message,
                            Result = response.Result,
                            StatusCode = response.StatusCode
                        });
                    }
                }
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User does not exist.",
                    Result = false,
                    StatusCode = 500
                });
            }
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Message = "Please provide vaild email/phone number.",
                Result = false,
                StatusCode = 500
            });
        }

        /// <summary>
        /// Send phone verification link
        /// </summary>
        /// <param name="countryCode"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="isUser"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("phone/send-verification-link")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendPhoneVerificationLink(string countryCode, string phoneNumber, bool isUser = true)
        {
            if (HttpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                isUser = false;
            }
            var user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.IsUser == isUser
                                && x.PhoneNumber == phoneNumber
                             )
                       .FirstOrDefault();

            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User does not exist.",
                    Result = false,
                    StatusCode = 500
                });
            }

            string token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
            string phoneVerificationUrl = _configuration["WebPages:PhoneVerificationUrl"] + $"?PhoneNumber={UtilityHelper.Base64Encode(user.PhoneNumber)}&Code={UtilityHelper.Base64Encode(token)}&IsUser={isUser}";
            var response = await _smsManager.SetPhoneVerificationLink(string.Concat(countryCode, phoneNumber), phoneVerificationUrl, user.Id.ToString());
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Message = response.Message,
                Result = response.Result
            });
        }

        /// <summary>
        /// Verify phone number
        /// </summary>
        /// <param name="PhoneNumber"></param>
        /// <param name="Code"></param>
        /// <param name="IsUser"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("phone/verify")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> PhoneVerification(string PhoneNumber, string Code, bool IsUser)
        {
            PhoneNumber = UtilityHelper.Base64Decode(PhoneNumber);
            Code = UtilityHelper.Base64Decode(Code);
            var user = _userManager
                       .Users
                       .Where(x => !x.IsDeleted
                                && x.IsUser == IsUser
                                && x.PhoneNumber == PhoneNumber
                             )
                       .FirstOrDefault();

            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User does not exist.",
                    Result = false,
                    StatusCode = 500
                });
            }
            var result = await _userManager.ChangePhoneNumberAsync(user, PhoneNumber, Code);
            if (!result.Succeeded)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "Invalid token",
                    Result = false,
                    StatusCode = 500
                });
            }
            else
            {
                await _itemTransferInvitationManager.InviteUserStatusUpdate(user);
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "Phone verification has been done successfully.",
                    Result = true
                });
            }

        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        [HttpGet("/api/users")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<UserResponseBaseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (!string.IsNullOrEmpty(pagedListCriteria.SearchText))
            {
                pagedListCriteria.SearchText = Uri.UnescapeDataString(pagedListCriteria.SearchText);
            }
            var mediaBaseUrl = _configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var user = _userManager
                      .GetUsersInRoleAsync(UserRole.User.ToString())
                      .Result
                      .Where(x => !x.IsDeleted)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList();

            var totalCount = user.Count();
            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "Data not found.",
                    Result = false,
                    StatusCode = 500
                });
            }
            if (!string.IsNullOrEmpty(pagedListCriteria.SearchText))
            {
                user = user.Where(x => (x.FirstName != null && x.FirstName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.LastName != null && x.LastName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.Email != null && x.Email.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.UserName != null && x.UserName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.PhoneNumber != null && string.Concat(x.CountryCode, x.PhoneNumber).Contains(pagedListCriteria.SearchText))).ToList();
                totalCount = user.Count();
            }
            var response = user.Select(x => new UserResponseBaseModel()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Username = x.UserName,
                Email = x.Email,
                EmailConfirmed = x.EmailConfirmed,
                PhoneNumber = string.Concat(x.CountryCode, x.PhoneNumber),
                PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                ProfileUrl = !string.IsNullOrEmpty(x.ProfileUrl) ? mediaBaseUrl + x.ProfileUrl : x.ProfileUrl,
                IsTemporaryLockOut = x.LockoutEnd.HasValue ? (x.LockoutEnd.Value.DateTime > DateTime.UtcNow && !x.IsPermanentLockOut ? true : false) : false,
                IsPermanentLocuOut = x.IsPermanentLockOut,
                IsBlockedByAdmin = x.IsUserBlockedByAdmin,
                IsVerifiedByAdmin = x.IsVerifiedByAdmin,
                CreateOn = x.CreatedDate
            })
            .Skip(pagedListCriteria.Skip)
            .Take(pagedListCriteria.Take)
            .ToList();

            return ResponseResult(new ManagerBaseResponse<IEnumerable<UserResponseBaseModel>>()
            {
                Result = response,
                IsSuccess = true,
                PageDetail = new PageDetailModel()
                {
                    Skip = pagedListCriteria.Skip,
                    Take = pagedListCriteria.Take,
                    Count = totalCount,
                    SearchText = pagedListCriteria.SearchText
                }
            });
        }

        /// <summary>
        /// Get all admin users
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("/api/admin-users")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<UserResponseBaseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAdminUsers([FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (!string.IsNullOrEmpty(pagedListCriteria.SearchText))
            {
                pagedListCriteria.SearchText = Uri.UnescapeDataString(pagedListCriteria.SearchText);
            }
            var mediaBaseUrl = _configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var user = _userManager
                      .GetUsersInRoleAsync(UserRole.Admin.ToString())
                      .Result
                      .Where(x => !x.IsDeleted)
                      .OrderByDescending(x => x.CreatedDate)
                      .ToList();

            var totalCount = user.Count();
            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "Data not found.",
                    Result = false,
                    StatusCode = 500
                });
            }
            if (!string.IsNullOrEmpty(pagedListCriteria.SearchText))
            {
                user = user.Where(x => (x.FirstName != null && x.FirstName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.LastName != null && x.LastName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.Email != null && x.Email.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.UserName != null && x.UserName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                    || (x.PhoneNumber != null && string.Concat(x.CountryCode, x.PhoneNumber).Contains(pagedListCriteria.SearchText))).ToList();
                totalCount = user.Count();
            }
            var response = user.Select(x => new UserResponseBaseModel()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Username = x.UserName.Replace("_Portal", string.Empty),
                Email = x.Email,
                EmailConfirmed = x.EmailConfirmed,
                PhoneNumber = string.Concat(x.CountryCode, x.PhoneNumber),
                PhoneNumberConfirmed = x.PhoneNumberConfirmed,
                ProfileUrl = !string.IsNullOrEmpty(x.ProfileUrl) ? mediaBaseUrl + x.ProfileUrl : x.ProfileUrl,
                IsTemporaryLockOut = x.LockoutEnd.HasValue ? (x.LockoutEnd.Value.DateTime > DateTime.UtcNow && !x.IsUserBlockedByAdmin ? true : false) : false,
                IsPermanentLocuOut = x.LockoutEnd.HasValue ? (x.LockoutEnd.Value.DateTime > DateTime.UtcNow && x.IsUserBlockedByAdmin ? true : false) : false,
                CreateOn = x.CreatedDate
            })
            .Skip(pagedListCriteria.Skip)
            .Take(pagedListCriteria.Take)
            .ToList();
            return ResponseResult(new ManagerBaseResponse<IEnumerable<UserResponseBaseModel>>()
            {
                Result = response,
                IsSuccess = true,
                PageDetail = new PageDetailModel()
                {
                    Skip = pagedListCriteria.Skip,
                    Take = pagedListCriteria.Take,
                    Count = totalCount,
                    SearchText = pagedListCriteria.SearchText
                }
            });
        }

        /// <summary>
        /// Create a new admin
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("/api/admin-users")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddAdminUser([FromBody] RegisterBaseModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
            if (model.UserName.Contains(" "))
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Username cannot contain white spaces."
                });
            }

            if (!string.IsNullOrEmpty(model.UserName))
            {
                var userName = _userManager
                               .Users
                               .Where(x => !x.IsDeleted
                                        && !x.IsUser
                                        && x.UserName.ToUpper() == string.Concat(model.UserName, "_Portal").ToUpper()
                                     )
                               .FirstOrDefault();

                if (userName != null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = "Username already exists."
                    });
                }
            }
            if (!string.IsNullOrEmpty(model.Email))
            {
                var email = _userManager
                            .Users
                            .Where(x => !x.IsDeleted
                                     && !x.IsUser
                                     && x.Email.ToUpper() == model.Email.ToUpper()
                                  )
                            .FirstOrDefault();

                if (email != null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = "Email already exists."
                    });
                }
            }
            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var phoneNumber = _userManager
                                  .Users
                                  .Where(x => !x.IsDeleted
                                           && !x.IsUser
                                           && x.PhoneNumber == model.PhoneNumber
                                         )
                                  .FirstOrDefault();

                if (phoneNumber != null)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        IsSuccess = false,
                        Result = false,
                        Message = "Phone number already exists."
                    });
                }
            }

            var user = new ApplicationUser()
            {
                UserName = string.Concat(model.UserName, "_Portal"),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                CountryCode = model.CountryCode,
                PhoneNumber = model.PhoneNumber,
                IsUser = false,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            IdentityResult result = await _userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, UserRole.Admin.ToString());
                var resetPasswordLink = await GenerateResetPasswordLink(user);
                await _emailManager.SetResetPasswordLinkEmail(string.Concat(user.FirstName, " ", user.LastName), user.Email, resetPasswordLink, user.Id);
                await SendEmailVerificationLink(user.Email, false);
                await SendPhoneVerificationLink(user.CountryCode, user.PhoneNumber, false);
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = true,
                    Result = true,
                    Message = "Account has been created successfully.",
                });
            }
            else
            {
                var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = message
                });
            }
        }

        /// <summary>
        /// User verification by admin
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isEmailVerification"></param>
        /// <param name="isPhoneVerification"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost("/api/admin/user-verification/{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> VerifyUser([FromRoute] string id, bool isEmailVerification, bool isPhoneVerification)
        {
            var user = _userManager.Users.Where(x => !x.IsDeleted && x.Id == id).FirstOrDefault();
            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User not found.",
                    Result = false,
                    StatusCode = 404
                });
            }
            if (isEmailVerification)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Message = message,
                        Result = false,
                        StatusCode = 500
                    });
                }
                user.IsVerifiedByAdmin = true;
                user.UpdatedDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _adminTransactionLogManager.AddTransactionLog(
                                                new AdminTransactionLogResponseModel()
                                                {
                                                    UserId = user.Id,
                                                    TransactionDescription = "Email verification",
                                                    OldStatus = "Unverified",
                                                    NewStatus = "Verified",
                                                }, User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            if (isPhoneVerification)
            {
                string token = await _userManager.GenerateChangePhoneNumberTokenAsync(user, user.PhoneNumber);
                var result = await _userManager.ChangePhoneNumberAsync(user, user.PhoneNumber, token);
                if (!result.Succeeded)
                {
                    var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Message = message,
                        Result = false,
                        StatusCode = 500
                    });
                }
                user.IsVerifiedByAdmin = true;
                user.UpdatedDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _adminTransactionLogManager.AddTransactionLog(
                                                new AdminTransactionLogResponseModel()
                                                {
                                                    UserId = user.Id,
                                                    TransactionDescription = "Phone number verification",
                                                    OldStatus = "Unverified",
                                                    NewStatus = "Verified",
                                                }, User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Message = "Verification has been done successfully.",
                Result = true
            });
        }

        /// <summary>
        /// Unlock user's account
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isTemporaryLock"></param>
        /// <param name="isPermanentLock"></param>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost("account-restriction/{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UserAccountRestriction([FromRoute] string id, bool isTemporaryLock, bool isPermanentLock)
        {
            var user = _userManager.Users.Where(x => !x.IsDeleted && x.Id == id).FirstOrDefault();
            if (user == null)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Message = "User not found.",
                    Result = false,
                    StatusCode = 404
                });
            }
            if (isTemporaryLock || isPermanentLock)
            {
                await _signInManager.UserManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(99));
                user.IsUserBlockedByAdmin = true;
                string message = string.Empty;
                if (isPermanentLock)
                {
                    user.AccessFailedCount = 9;
                    user.IsPermanentLockOut = true;
                    message = "account has been permanent locked";
                }
                else
                {
                    user.IsPermanentLockOut = false;
                    message = "account has been temporary locked";
                }
                user.UpdatedDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _adminTransactionLogManager.AddTransactionLog(
                                                    new AdminTransactionLogResponseModel()
                                                    {
                                                        UserId = user.Id,
                                                        TransactionDescription = string.Concat(user.FirstName, " ", user.LastName, "'s " + message),
                                                        OldStatus = "Unlock",
                                                        NewStatus = "Lock",
                                                    }, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = true,
                    Message = "User " + message + " successfully."
                });

            }
            else
            {
                await _signInManager.UserManager.ResetAccessFailedCountAsync(user);
                await _signInManager.UserManager.SetLockoutEndDateAsync(user, null);
                user.IsUserBlockedByAdmin = false;
                if (!isPermanentLock)
                {
                    user.IsPermanentLockOut = false;
                }
                user.UpdatedDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);
                await _adminTransactionLogManager.AddTransactionLog(
                                                    new AdminTransactionLogResponseModel()
                                                    {
                                                        UserId = user.Id,
                                                        TransactionDescription = string.Concat(user.FirstName, " ", user.LastName, "'s account has been unlocked"),
                                                        OldStatus = "Lock",
                                                        NewStatus = "Unlock",
                                                    }, User.FindFirstValue(ClaimTypes.NameIdentifier));
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    Result = true,
                    Message = "User account has been unlocked successfully."
                });
            }
        }

        /// <summary>
        /// Confirm Password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost("/api/confirm-password/{password}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> IsPasswordExists([FromRoute] string password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                var user = _userManager.Users.FirstOrDefault(x => !x.IsDeleted && x.Id == User.Claims.GetUserId());
                if (user != null)
                {
                    var isExists = await _userManager.CheckPasswordAsync(user, password);
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Result = isExists,
                        Message = isExists ? null : "The password doesnt match our records"
                    });
                }
            }

            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Result = false,
                Message = "The password doesnt match our records",
            });
        }

        /// <summary>
        /// Delete account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new Exception("Requested id is not valid.");
            }
            var user = _userManager.Users.Where(x => !x.IsDeleted && x.Id == id).FirstOrDefault();
            if (user != null)
            {
                var tempString = Guid.NewGuid().ToString().Substring(0, 8);
                user.UserName = user.UserName + "_" + tempString;
                user.NormalizedUserName = user.NormalizedUserName + "_" + tempString;
                user.Email = user.Email + "_" + tempString;
                user.NormalizedEmail = user.NormalizedEmail + "_" + tempString;
                user.EmailConfirmed = false;
                user.PhoneNumber = user.PhoneNumber + "_" + tempString;
                user.PhoneNumberConfirmed = false;
                user.IsDeleted = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    #region Delete User's Locations, Products and associated details

                    List<long> locationIds = _unitOfWork
                                            .LocationsRepository
                                            .GetQueryable(x => !x.IsDeleted && x.CreatedBy == user.Id)
                                            .Select(x => x.Id)
                                            .ToList();
                    if (locationIds.Any())
                    {
                        await _locationsManager.DeleteLocation(locationIds, user.Id, "DeleteAccount");
                    }

                    #endregion

                    #region Delete User's Addresses

                    var addressEntity = _unitOfWork
                                       .AddressesRepository
                                       .GetQueryable(entity => !entity.IsDeleted && entity.CreatedBy == user.Id)
                                       .ToList();

                    if (addressEntity.Any())
                    {
                        foreach (var address in addressEntity)
                        {
                            address.IsDeleted = true;
                            address.DeletedOn = DateTime.UtcNow;
                            address.DeletedBy = user.Id;
                            await _unitOfWork.CommitAsync();
                        }
                    }

                    #endregion

                    #region Delete Registered Devices

                    var RegisterDevices = _unitOfWork.RegisterDeviceRepository
                                           .GetQueryable(entity => !entity.IsDeleted && entity.UserId == id)
                                           .ToList();
                    if (RegisterDevices.Any())
                    {
                        foreach (var RegisterDevice in RegisterDevices)
                        {
                            RegisterDevice.IsDeleted = true;
                            RegisterDevice.DeletedBy = id;
                            RegisterDevice.DeletedOn = DateTime.UtcNow;
                            await _unitOfWork.CommitAsync();
                        }
                    }

                    #endregion

                    #region Delete Transfered Items
                    //var transferedItems = await _unitOfWork.ItemTransferRepository
                    //                   .GetQueryable(x => !x.IsDeleted && (x.FromUserId == user.Id || x.ToUserId == user.Id))
                    //                   .ToListAsync();

                    //if (transferedItems.Any())
                    //{
                    //    foreach (var item in transferedItems)
                    //    {
                    //        item.IsDeleted = true;
                    //        item.DeletedOn = DateTime.UtcNow;
                    //        item.DeletedBy = user.Id;
                    //        await _unitOfWork.CommitAsync();
                    //    }
                    //}
                    #endregion

                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Message = "Account has been deleted successfully.",
                        Result = true
                    });
                }
                else
                {
                    var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Result = false,
                        Message = message,
                        StatusCode = 500
                    });
                }
            }
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Result = false,
                Message = "User not found",
                StatusCode = 404
            });
        }

        /// <summary>
        /// Update verification reminders count and time
        /// </summary>
        /// <returns></returns>
        [HttpPost("verification-reminder")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateUsersAccount()
        {
            var user = _userManager.Users.Where(x => !x.IsDeleted && x.Id == User.Claims.GetUserId()).FirstOrDefault();
            if (user != null)
            {
                if (user.IsFirstLoginAttempt)
                {
                    user.IsFirstLoginAttempt = false;
                }
                user.VerificationRemindedOn = DateTime.UtcNow;
                user.VerificationReminderCount++;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Result = true,
                    });
                }
                else
                {
                    var message = string.Join(" | ", result.Errors.Select(x => x.Description));
                    return ResponseResult(new ManagerBaseResponse<bool>()
                    {
                        Result = false,
                        Message = message,
                        StatusCode = 500
                    });
                }
            }
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Result = false,
                Message = "User not found",
                StatusCode = 404
            });
        }

        /// <summary>
        /// Send item transfer invitations to the users who are not part of the application
        /// </summary>
        /// <returns></returns>
        [HttpPost("invite-user")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> InviteUser(ItemTransferInvitationRequestModel model)
        {
            return ResponseResult(await _itemTransferInvitationManager.InviteUser(model, User.Claims.GetUserId()));
        }
        async Task<AuthenticateModel> GetAuthTokenResponse(ApplicationUser user, bool isSocialLogin = false)
        {
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }
            if (!user.EmailConfirmed && !user.PhoneNumberConfirmed)
            {
                return new AuthenticateModel()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    CurrencyCode = user.CurrencyCode,
                    CountryCode = user.CountryCode,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    Username = user.UserName.Replace("_Portal", string.Empty),
                    IsSocialLogIn = isSocialLogin,
                    IsFirstLoginAttempt = user.IsFirstLoginAttempt,
                    VerificationRemindedOn = user.VerificationRemindedOn,
                    VerificationReminderCount = user.VerificationReminderCount
                };
            }
            var role = _userManager.GetRolesAsync(user).Result.FirstOrDefault();
            var tokenData = GetToken(user.UserName, user.Id, role);
            var generateRefreshToken = UtilityHelper.GenerateRefreshToken();

            await _userRefreshTokenManager.Add(generateRefreshToken, user.Id, GetIpAddress(), DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["RefreshTokenExpireInDays"])));

            return new AuthenticateModel()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                CurrencyCode = user.CurrencyCode,
                CountryCode = user.CountryCode,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                Username = user.UserName.Replace("_Portal", string.Empty),
                Role = role,
                ValidFrom = tokenData.ValidFrom,
                ValidTo = tokenData.ValidTo,
                Token = new JwtSecurityTokenHandler().WriteToken(tokenData),
                RefreshToken = generateRefreshToken,
                RefreshTokenExpiryTime = DateTime.Now.AddDays(Convert.ToDouble(_configuration["RefreshTokenExpireInDays"])),
                IsSocialLogIn = isSocialLogin,
                IsFirstLoginAttempt = user.IsFirstLoginAttempt,
                VerificationRemindedOn = user.VerificationRemindedOn,
                VerificationReminderCount = user.VerificationReminderCount
            };
        }
        SecurityToken GetToken(string username, string userId, string role)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWTSecretKey"]);
            DateTime ExpireDateTime = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["AccessTokenExpireInMinutes"]));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                     new Claim(ClaimTypes.Name, username)
                    ,new Claim(ClaimTypes.NameIdentifier, userId)
                    ,new Claim(ClaimTypes.Role, role)
                }),
                Expires = ExpireDateTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            return tokenHandler.CreateToken(tokenDescriptor);
        }
        string GetIpAddress()
        {
            // get source ip address for the current request
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
        public static bool IsStringContainsEmail(string str)
        {
            return Regex.IsMatch(str, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
        }
        private bool IsEmailAlreadyExist(string email, string userId, bool isUser)
        {
            return _userManager.Users.Any(x => x.Email.ToLower() == email.ToLower()
                                            && x.NormalizedEmail == email.ToUpper()
                                            && x.Id != userId
                                            && x.IsUser == isUser);
        }
        private bool IsPhoneAlreadyExist(string phone, string userId, bool isUser)
        {
            return _userManager.Users.Any(x => x.PhoneNumber == phone
                                            && x.Id != userId
                                            && x.IsUser == isUser);
        }
        private bool IsUserNameAlreadyExist(string userName, string userId, bool isUser)
        {
            if (!isUser)
            {
                userName = string.Concat(userName, "_Portal");
            }
            return _userManager.Users.Any(x => x.UserName.ToLower() == userName.ToLower()
                                            && x.NormalizedUserName == userName.ToUpper()
                                            && x.Id != userId
                                            && x.IsUser == isUser);
        }
        private async Task<string> GenerateResetPasswordLink(ApplicationUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPasswordURL = _configuration["WebPages:ResetPasswordPageUrl"] + $"?Email={UtilityHelper.Base64Encode(user.Email)}&Code={UtilityHelper.Base64Encode(token)}&Date={UtilityHelper.Base64Encode(DateTime.Now.ToString("yyyy-MM-dd hh:mm"))}";
            return resetPasswordURL;
        }
    }
}
