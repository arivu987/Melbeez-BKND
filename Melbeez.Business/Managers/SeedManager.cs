using Melbeez.Business.Managers.Abstractions;
using Melbeez.Common.Helpers;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class SeedManager : ISeedManager
    {
        private readonly ILogger<SeedManager> logger;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;

        public SeedManager(
            ILogger<SeedManager> logger,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager
        )
        {
            this.logger = logger;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        /// <summary>
        /// Default Seed method which contains all seed methods
        /// </summary>
        /// <returns></returns>
        public async Task SeedAsync()
        {
            await SeedRolesAsync();
            await SeedUsersAsync();
        }

        /// <summary>
        /// Create default users with super admin role
        /// </summary>
        /// <returns></returns>
        private async Task SeedUsersAsync()
        {
            try
            {
                var users = new List<ApplicationUser>
                {
                    new ApplicationUser()
                    {
                        FirstName = "Senthil",
                        LastName = "Kumar",
                        Email = "kpsent@gmail.com",
                        NormalizedEmail = "KPSENT@GMAIL.COM",
                        EmailConfirmed = true,
                        UserName = "Senthil.Kumar",
                        NormalizedUserName = "SENTHIL.KUMAR",
                        PhoneNumber = "3037487001",
                        PhoneNumberConfirmed = true,
                        CountryCode = "+1",
                        CurrencyCode = "USD",
                        CreatedDate = DateTime.UtcNow,
                        VerificationRemindedOn = DateTime.UtcNow,
                        VerificationReminderCount = 3
                    },
                    new ApplicationUser()
                    {
                        FirstName = "Prakash",
                        LastName = "J",
                        Email = "prakashjay@gmail.com",
                        NormalizedEmail = "PRAKASHJAY@GMAIL.COM",
                        EmailConfirmed = true,
                        UserName = "Prakash",
                        NormalizedUserName = "PRAKASH",
                        PhoneNumber = "1234567890",
                        PhoneNumberConfirmed = false,
                        CountryCode = "+91",
                        CurrencyCode = "USD",
                        CreatedDate = DateTime.UtcNow,
                        VerificationRemindedOn = DateTime.UtcNow,
                        VerificationReminderCount = 3
                    }
                };
                foreach (var user in users)
                {
                    var userName = userManager.Users.Where(x => !x.IsDeleted && x.UserName.ToUpper() == user.UserName.ToUpper()).FirstOrDefault()?.UserName;
                    var email = userManager.Users.Where(x => !x.IsDeleted && x.Email.ToUpper() == user.Email.ToUpper()).FirstOrDefault()?.Email;
                    var phoneNumber = userManager.Users.Where(x => !x.IsDeleted && x.PhoneNumber == user.PhoneNumber).FirstOrDefault()?.PhoneNumber;

                    if (string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phoneNumber))
                    {
                        IdentityResult result = await userManager.CreateAsync(user, "Melbeez@123");
                        if (result.Succeeded) 
                        {
                            await userManager.AddToRoleAsync(user, UserRole.SuperAdmin.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in roles seeding");
            }
        }

        /// <summary>
        /// Create default roles
        /// </summary>
        /// <returns></returns>
        private async Task SeedRolesAsync()
        {
            try
            {
                var roles = new List<ApplicationRole>
                {
                    new ApplicationRole()
                    {
                        Name = "Admin",
                        NormalizedName = "ADMIN"
                    },
                    new ApplicationRole()
                    {
                        Name = "User",
                        NormalizedName = "USER"
                    },
                    new ApplicationRole()
                    {
                        Name = "SuperAdmin",
                        NormalizedName = "SUPERADMIN"
                    }
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role.Name))
                    {
                        await roleManager.CreateAsync(role);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in roles seeding");
            }
        }
    }
}
