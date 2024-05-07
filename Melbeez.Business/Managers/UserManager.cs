using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Common.Helpers;
using Melbeez.Domain.Entities.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Business.Models.UserModels.RequestModels;

namespace Melbeez.Business.Managers
{
    public class UserManager : IUserManager
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IEmailManager emailManager;
        private readonly IConfiguration configuration;

        public UserManager(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IEmailManager emailManager,
            IConfiguration configuration
        )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.emailManager = emailManager;
            this.configuration = configuration;
        }
        public async Task<CreateUserResponseModel> CreateUser(ApplicationUser user)
        {
            var result = await userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                //Send an Email about the Account Creation and Password change link.
                var code = await userManager.GeneratePasswordResetTokenAsync(user);
                code = UtilityHelper.Base64Encode(code);
                var email = UtilityHelper.Base64Encode(user.Email);

                string resetPasswordUrl = configuration["WebPages:ResetPasswordPageUrl"];
                resetPasswordUrl = resetPasswordUrl + $"?Email={email}&code={code}";

                await emailManager.SetResetPasswordLinkEmail(string.Concat(user.FirstName, " ", user.LastName), email, resetPasswordUrl, user.Id);

                return new CreateUserResponseModel()
                {
                    IsSuccess = true,
                    Message = "User account created successfully. Please check your email for set password.",
                    User = user,
                };
            }
            return new CreateUserResponseModel()
            {
                IsSuccess = true,
                Message = string.Join(" | ", result.Errors.Select(x => x.Description))
            };
        }
        public async Task<CreateUserResponseModel> CreateUser(ApplicationUser user, string password)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return new CreateUserResponseModel()
                {
                    IsSuccess = true,
                    Message = "User account created successfully.",
                    User = user,
                };
            }
            return new CreateUserResponseModel()
            {
                IsSuccess = true,
                Message = string.Join(" | ", result.Errors.Select(x => x.Description))
            };
        }
        public async Task<CreateUserResponseModel> CreateExternalProviderUser(ApplicationUser user, string provider, string providerId)
        {
            var result = await userManager.CreateAsync(user);

            if (result.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole
                    {
                        Name = "User"
                    });
                }
                await userManager.AddToRoleAsync(user, "User");

                var userLoginInfo = new UserLoginInfo(provider, providerId, provider);
                var resp = await userManager.AddLoginAsync(user, userLoginInfo);
                if (resp.Succeeded)
                {
                    return new CreateUserResponseModel()
                    {
                        IsSuccess = true,
                        Message = "User account created successfully.",
                        User = user,
                    };
                }
                return new CreateUserResponseModel()
                {
                    IsSuccess = true,
                    Message = string.Join(" | ", resp.Errors.Select(x => x.Description))
                };
            }
            return new CreateUserResponseModel()
            {
                IsSuccess = true,
                Message = string.Join(" | ", result.Errors.Select(x => x.Description))
            };
        }
        public ManagerBaseResponse<IEnumerable<Models.AccountModels.RoleModel>> GetRoleList()
        {
            var role = roleManager.Roles.ToList();
            var response = role.Select(x => new Models.AccountModels.RoleModel
            {
                Id = x.Id,
                Name = x.Name,
                NormalizedName = x.NormalizedName,
            });

            return new ManagerBaseResponse<IEnumerable<Models.AccountModels.RoleModel>>()
            {
                Result = response
            };
        }
        public async Task<ManagerBaseResponse<bool>> UpdateUser(string id, ApplicationUserUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Invalid requested user id"
                };
            }

            var findUser = await userManager.FindByIdAsync(id);
            if (findUser == null)
            {
                return new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Invalid requested user id"
                };
            }

            if (model == null)
            {
                return new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Invalid requested user id"
                };
            }

            findUser.FirstName = model.FirstName;
            findUser.LastName = model.LastName;
            findUser.PhoneNumber = model.PhoneNumber;

            await userManager.UpdateAsync(findUser);

            return new ManagerBaseResponse<bool>()
            {
                IsSuccess = true,
                Result = true,
                Message = "Record updated successfully"
            };
        }
        public async Task<ManagerBaseResponse<bool>> UpdateUserByEmail(string email, ApplicationUserUpdateModel model)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Invalid requested user email"
                };
            }

            var findUser = await userManager.FindByEmailAsync(email);
            if (findUser == null)
            {
                return new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Invalid requested user email"
                };
            }

            if (model == null)
            {
                return new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = "Invalid requested payload"
                };
            }

            findUser.FirstName = model.FirstName;
            findUser.LastName = model.LastName;
            findUser.PhoneNumber = model.PhoneNumber;

            await userManager.UpdateAsync(findUser);

            return new ManagerBaseResponse<bool>()
            {
                IsSuccess = true,
                Result = true,
                Message = "Record updated successfully."
            };
        }
    }
}
