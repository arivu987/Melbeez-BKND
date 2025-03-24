using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Common.Services;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class SendNotificationManager : ISendNotificationManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IRegisterDeviceManager registerDeviceManager;
        private readonly IPushNotificationManager pushNotificationManager;
        private readonly IConfiguration configuration;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserNotificationPreferenceManager userNotificationPreferenceManager;
        public SendNotificationManager(IUnitOfWork unitOfWork,
                                       IRegisterDeviceManager registerDeviceManager,
                                       IPushNotificationManager pushNotificationManager,
                                       IConfiguration configuration,
                                       UserManager<ApplicationUser> userManager,
                                       IUserNotificationPreferenceManager userNotificationPreferenceManager)
        {
            this.unitOfWork = unitOfWork;
            this.registerDeviceManager = registerDeviceManager;
            this.pushNotificationManager = pushNotificationManager;
            this.configuration = configuration;
            this.userManager = userManager;
            this.userNotificationPreferenceManager = userNotificationPreferenceManager;
        }
        public async Task<ManagerBaseResponse<bool>> SendWarrentyExpiryNotification(string userId)
        {
            DateTime startDate = DateTime.UtcNow.Date.AddDays(-1);
            DateTime endDate = startDate.AddDays(61);
            var response = await unitOfWork.ProductWarrantiesRepository
                .GetQueryable(x => !x.IsDeleted && x.EndDate.Date >= startDate.Date && x.EndDate.Date <= endDate.Date)
                .AsNoTracking()
                .ToListAsync();

            if (response.Any())
            {
                foreach (var item in response)
                {
                    PushNotificationRequestModel model = new()
                    {
                        RecipientId = item.CreatedBy,
                        NotificationType = NotificationType.WarrantyExpiry
                    };
                    var DeviceTokens = await GetDeviceTokens(item.CreatedBy);

                    DateTime endDateWithOffset = item.EndDate;
                    DateTime currentDate = DateTime.UtcNow;
                    TimeSpan timeRemaining = endDateWithOffset - currentDate;
                    int daysRemaining = (int)Math.Ceiling(timeRemaining.TotalDays);

                    if (daysRemaining <= 0)
                    {
                        model.ReferenceId = item.ProductId.ToString();
                        model.Title = "Warranty is expired";
                        model.Description = "Your warranty for " + item.Name + " has expired on " + item.EndDate.ToString("dd-MM-yyyy");
                    }
                    else
                    {
                        if (daysRemaining == 60 || daysRemaining == 30 || daysRemaining == 1)
                        {
                            model.ReferenceId = item.ProductId.ToString();
                            model.Title = daysRemaining == 1 ? "Warranty about to expire!" : "Warranty about to expire in " + daysRemaining + " days!";
                            model.Description = "Your warranty for " + item.Name + " will expire on " + item.EndDate.ToString("dd-MM-yyyy");
                        }
                    }

                    if (!string.IsNullOrEmpty(model.Title))
                    {
                        if (await GetUserNotificationPreference(item.CreatedBy, "WarrentyExpiryNotification") && DeviceTokens.Any())
                        {
                            await SendAndSaveNotification(model, DeviceTokens, userId);
                        }
                        else
                        {
                            await SaveNotification(model, userId);
                        }
                    }
                }

                return new ManagerBaseResponse<bool>()
                {
                    Result = true,
                    Message = "Notification sent successfully.",
                };
            }

            return new ManagerBaseResponse<bool>()
            {
                Result = false,
                Message = "No expired warranty found.",
            };
        }
        public async Task<ManagerBaseResponse<bool>> SendLocationUpdateNotification(PushNotificationRequestModel model, string userId)
        {
            try
            {
                if (await GetUserNotificationPreference(userId, "LocationUpdateNotification"))
                {
                    var deviceTokens = await GetDeviceTokens(userId);

                    if (deviceTokens == null || !deviceTokens.Any())
                    {
                        return new ManagerBaseResponse<bool>()
                        {
                            Message = "No device tokens found for the user.",
                            Result = false,
                            StatusCode = 400
                        };
                    }

                    model.RecipientId = userId;
                    return await SendAndSaveNotification(model, deviceTokens, userId);
                }
                else
                {
                    model.RecipientId = userId;
                    return await SaveNotification(model, userId);
                }
            }
            catch (Exception ex)
            {

                return new ManagerBaseResponse<bool>()
                {
                    Message = $"Failed to send notification: {ex.Message}",
                    Result = false,
                    StatusCode = 500
                };
            }
        }

        public async Task<ManagerBaseResponse<bool>> SendProductUpdateNotification(PushNotificationRequestModel model, string userId)
        {
            try
            {
                if (await GetUserNotificationPreference(userId, "ProductUpdateNotification"))
                {
                    var DeviceTokens = await GetDeviceTokens(userId);
                    if (DeviceTokens == null || !DeviceTokens.Any())
                    {
                        return new ManagerBaseResponse<bool>()
                        {
                            Message = "No device tokens found for the user.",
                            Result = false,
                            StatusCode = 400
                        };
                    }
                    model.RecipientId = userId;
                    return await SendAndSaveNotification(model, DeviceTokens, userId);
                }
                else
                {
                    model.RecipientId = userId;
                    return await SaveNotification(model, userId);
                }
            }
            catch (Exception ex)
            {

                return new ManagerBaseResponse<bool>()
                {
                    Message = $"Failed to send notification: {ex.Message}",
                    Result = false,
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> SendDeviceActivationNotification(PushNotificationRequestModel model, string userId)
        {
            //TODO :: It is not in scope
            return new ManagerBaseResponse<bool>()
            {
                Result = false,
            };
        }
        public async Task<ManagerBaseResponse<bool>> SendMarketingAlertNotification(PushNotificationRequestModel model, string userId)
        {
            var users = await userManager.GetUsersInRoleAsync(UserRole.User.ToString());
            if (users.Any())
            {
                foreach (var user in users)
                {
                    if (await GetUserNotificationPreference(user.Id, "MarketingAlertNotification"))
                    {
                        var DeviceTokens = await GetDeviceTokens(user.Id);
                        model.RecipientId = user.Id;
                        await SendAndSaveNotification(model, DeviceTokens, userId);
                    }
                    else
                    {
                        model.RecipientId = user.Id;
                        await SaveNotification(model, userId);
                    }
                }
                return new ManagerBaseResponse<bool>()
                {
                    Result = true,
                    Message = "Notification sent successfully.",
                };
            }
            return new ManagerBaseResponse<bool>()
            {
                Result = false,
                Message = "No user found.",
                StatusCode = 500
            };
        }

        private async Task<ManagerBaseResponse<bool>> SendAndSaveNotification(PushNotificationRequestModel model, List<string> DeviceTokens, string userId)
        {
            string firebaseAccount = configuration["FireBaseNotification:FireBase.Accounts"];
            string firebaseUrl = configuration["FireBaseNotification:FireBase.RequestUrl"];

            // Ensure we return a response even if no devices are found
            if (DeviceTokens == null || !DeviceTokens.Any())
            {
                return new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "No devices are found",
                    StatusCode = 500
                };
            }

            string responseData = string.Empty; // ✅ Move responseData outside try for better debugging

            try
            {
                responseData = await NotificationService.Send(DeviceTokens, model.Title, model.Description, model.NotificationType.ToString(), model.ReferenceId, userId, firebaseAccount, firebaseUrl);

                if (string.IsNullOrWhiteSpace(responseData))
                {
                    throw new Exception("Firebase response is empty or null.");
                }

                Console.WriteLine($"Firebase Response: {responseData}");

                // ✅ Deserialize response while handling unexpected formats
                var item = JsonConvert.DeserializeObject<FirebaseNotificationResponse>(responseData);

                if (item == null)
                {
                    throw new Exception("Failed to parse Firebase response.");
                }

                // ✅ Check if the notification was successfully sent
                if (!string.IsNullOrEmpty(item.name))
                {
                    Console.WriteLine($"Notification sent successfully. Message ID: {item.name}");

                    await pushNotificationManager.Add(new PushNotificationModel()
                    {
                        RecipientId = model.RecipientId,
                        Title = model.Title,
                        Description = model.Description,
                        Type = model.NotificationType,
                        IsSuccess = true,  // ✅ Mark as successful
                        IsRead = false,
                        ReferenceId = string.IsNullOrEmpty(model.ReferenceId) ? null : model.ReferenceId,
                        ExpiryDate = null,
                        ErrorMeassge = null,
                        Status = model.Status == null ? null : (MovedStatus)model.Status
                    }, userId);

                    return new ManagerBaseResponse<bool>
                    {
                        Result = true,
                        Message = "Notification sent successfully."
                    };
                }

                // ✅ Handle the old API response format with `results`
                if (item.results != null && item.results.Any())
                {
                    var firstError = item.results.Select(s => s.error).FirstOrDefault();

                    await pushNotificationManager.Add(new PushNotificationModel()
                    {
                        RecipientId = model.RecipientId,
                        Title = model.Title,
                        Description = model.Description,
                        Type = model.NotificationType,
                        IsSuccess = false,
                        IsRead = false,
                        ReferenceId = string.IsNullOrEmpty(model.ReferenceId) ? null : model.ReferenceId,
                        ExpiryDate = null,
                        ErrorMeassge = firstError,
                        Status = model.Status == null ? null : (MovedStatus)model.Status
                    }, userId);

                    return new ManagerBaseResponse<bool>
                    {
                        Result = false,
                        Message = string.IsNullOrEmpty(firstError) ? "Notification failed" : firstError
                    };
                }

                throw new Exception("Unexpected Firebase response format.");
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"JSON Parse Error: {ex.Message}");
                Console.WriteLine($"Response Data: {responseData}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
            }

            // ✅ Default failure response if Firebase service fails
            return new ManagerBaseResponse<bool>()
            {
                Result = false,
                Message = "Notification service not responding.",
                StatusCode = 500
            };
        }


        private async Task<ManagerBaseResponse<bool>> SaveNotification(PushNotificationRequestModel model, string userId)
        {

            await pushNotificationManager.Add(new PushNotificationModel()
            {
                RecipientId = model.RecipientId,
                Title = model.Title,
                Description = model.Description,
                Type = model.NotificationType,
                IsSuccess = true,
                IsRead = false,
                ReferenceId = null,
                ExpiryDate = null,
                ErrorMeassge = null,
                Status = model.Status == null ? null : (MovedStatus)model.Status
            }, userId);

            return new ManagerBaseResponse<bool>()
            {
                Result = true,
                Message = "Notification saved successfully."
            };
        }
        private async Task<List<string>> GetDeviceTokens(string userId = null)
        {
            var RegDevice = await registerDeviceManager.Get();
            List<string> DeviceTokens = new List<string>();
            if (!string.IsNullOrEmpty(userId))
            {
                DeviceTokens = RegDevice.Result
                                        .Where(u => u.UserId == userId)
                                        .Select(x => x.DeviceToken)
                                        .ToList();
            }
            else
            {
                DeviceTokens = RegDevice.Result
                                        .Select(x => x.DeviceToken)
                                        .ToList();
            }
            return DeviceTokens;
        }
        private async Task<bool> GetUserNotificationPreference(string userId, string callingFrom)
        {
            bool result = false;
            var userNotificationPreferenceList = await userNotificationPreferenceManager.Get(userId);
            if (callingFrom == "WarrentyExpiryNotification")
            {
                result = userNotificationPreferenceList.Result.IsWarrantyExpireAlert == true
                        && userNotificationPreferenceList.Result.IsPushNotification == true
                        ? true : false;
            }
            else if (callingFrom == "LocationUpdateNotification")
            {
                result = userNotificationPreferenceList.Result.IsLocationUpdateAlert == true
                        && userNotificationPreferenceList.Result.IsPushNotification == true
                        ? true : false;
            }
            else if (callingFrom == "ProductUpdateNotification")
            {
                result = userNotificationPreferenceList.Result.IsProductUpdateAlert == true
                        && userNotificationPreferenceList.Result.IsPushNotification == true
                        ? true : false;
            }
            else if (callingFrom == "DeviceActivationNotification")
            {
                result = userNotificationPreferenceList.Result.IsDeviceActivationAlert == true
                        && userNotificationPreferenceList.Result.IsPushNotification == true
                        ? true : false;
            }
            else if (callingFrom == "MarketingAlertNotification")
            {
                result = userNotificationPreferenceList.Result.IsMarketingValueAlert == true
                        && userNotificationPreferenceList.Result.IsPushNotification == true
                        ? true : false;
            }
            return result;
        }
        public async Task<ManagerBaseResponse<bool>> SendItemTransferNotification(PushNotificationRequestModel model, string userId)
        {
            var DeviceTokens = await GetDeviceTokens(model.RecipientId);
            return await SendAndSaveNotification(model, DeviceTokens, userId);
        }
    }
}
