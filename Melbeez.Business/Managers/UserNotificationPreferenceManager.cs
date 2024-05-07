using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Data.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class UserNotificationPreferenceManager : IUserNotificationPreferenceManager
    {
        private readonly IUnitOfWork unitOfWork;
        public UserNotificationPreferenceManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<UserNotificationPreferenceResponseModel>> Get(string userId)
        {
            var result = unitOfWork
                .UserNotificationPreferenceRepository
                .GetAsync(x => !x.IsDeleted && x.UserId == userId).Result;

            if (result != null)
            {
                var setResponseModel = new UserNotificationPreferenceResponseModel()
                {
                    Id = result.Id,
                    IsWarrantyExpireAlert = result.IsWarrantyExpireAlert,
                    IsLocationUpdateAlert = result.IsLocationUpdateAlert,
                    IsProductUpdateAlert = result.IsProductUpdateAlert,
                    IsDeviceActivationAlert = result.IsDeviceActivationAlert,
                    IsMarketingValueAlert = result.IsMarketingValueAlert,
                    IsPushNotification = result.IsPushNotification,
                    IsEmailNotification = result.IsEmailNotification,
                    IsTextNotification = result.IsTextNotification,
                    IsThirdPartyServiceAllowed = result.IsThirdPartyServiceAllowed,
                    IsBiometricAllowed = result.IsBiometricAllowed
                };
                return new ManagerBaseResponse<UserNotificationPreferenceResponseModel>()
                {
                    Result = setResponseModel
                };
            }

            return new ManagerBaseResponse<UserNotificationPreferenceResponseModel>()
            {
                Result = null
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddUserNotificationPreference(UserNotificationPreferenceResponseModel model, string userId)
        {
            try
            {
                await unitOfWork.UserNotificationPreferenceRepository.AddAsync(new Domain.Entities.UserNotificationPreferenceEntity()
                {
                    UserId = userId,
                    IsWarrantyExpireAlert = model.IsWarrantyExpireAlert,
                    IsLocationUpdateAlert = model.IsLocationUpdateAlert,
                    IsProductUpdateAlert = model.IsProductUpdateAlert,
                    IsDeviceActivationAlert = model.IsDeviceActivationAlert,
                    IsMarketingValueAlert = model.IsMarketingValueAlert,
                    IsPushNotification = model.IsPushNotification,
                    IsEmailNotification = model.IsEmailNotification,
                    IsTextNotification = model.IsTextNotification,
                    IsThirdPartyServiceAllowed = model.IsThirdPartyServiceAllowed,
                    IsBiometricAllowed = model.IsBiometricAllowed,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                });

                await unitOfWork.CommitAsync();

                return new ManagerBaseResponse<bool>()
                {
                    Message = "User notification preference added successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false,
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateUserNotificationPreference(UserNotificationPreferenceResponseModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                            .UserNotificationPreferenceRepository
                            .GetAsync(entity => !entity.IsDeleted && entity.Id == model.Id && entity.UserId == userId);

                if (entity != null)
                {
                    entity.UserId = userId;
                    entity.IsWarrantyExpireAlert = model.IsWarrantyExpireAlert;
                    entity.IsLocationUpdateAlert = model.IsLocationUpdateAlert;
                    entity.IsProductUpdateAlert = model.IsProductUpdateAlert;
                    entity.IsDeviceActivationAlert = model.IsDeviceActivationAlert;
                    entity.IsMarketingValueAlert = model.IsMarketingValueAlert;
                    entity.IsPushNotification = model.IsPushNotification;
                    entity.IsEmailNotification = model.IsEmailNotification;
                    entity.IsTextNotification = model.IsTextNotification;
                    entity.IsThirdPartyServiceAllowed = model.IsThirdPartyServiceAllowed;
                    entity.IsBiometricAllowed = model.IsBiometricAllowed;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
                else
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Invalid user notification preference request.",
                        Result = false,
                        StatusCode = 500
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "User notification preference updated successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false,
                    StatusCode = 500
                };
            }
        }
    }
}