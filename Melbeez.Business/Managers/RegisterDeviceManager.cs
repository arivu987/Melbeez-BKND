using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models.Entities;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class RegisterDeviceManager : IRegisterDeviceManager
    {
        private readonly IUnitOfWork unitOfWork;
        public RegisterDeviceManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<RegisterDeviceResponseModel>>> Get()
        {
            var result = await unitOfWork
                .RegisterDeviceRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new RegisterDeviceResponseModel()
                {
                    Id = x.Id,
                    UId = x.UId,
                    UserId = x.UserId,
                    DeviceToken = x.DeviceToken,
                    DeviceType = x.DeviceType,
                    AppVersion = x.AppVersion,
                    OSVersion = x.OSVersion,
                    Lat = x.Lat,
                    Long = x.Long
                })
                .AsNoTracking()
                .ToListAsync();
            return new ManagerBaseResponse<IEnumerable<RegisterDeviceResponseModel>>()
            {
                Result = result
            };
        }

        public async Task<ManagerBaseResponse<IEnumerable<RegisterDeviceResponseModel>>> Get(string uid, string userId)
        {
            var result = await unitOfWork
                .RegisterDeviceRepository
                .GetQueryable(x => !x.IsDeleted && x.UserId == userId && x.UId == uid)
                .Select(x => new RegisterDeviceResponseModel()
                {
                    Id = x.Id,
                    UId = x.UId,
                    UserId = x.UserId,
                    DeviceToken = x.DeviceToken,
                    DeviceType = x.DeviceType,
                    AppVersion = x.AppVersion,
                    OSVersion = x.OSVersion,
                    Lat = x.Lat,
                    Long = x.Long
                })
                .AsNoTracking()
                .ToListAsync();
            return new ManagerBaseResponse<IEnumerable<RegisterDeviceResponseModel>>()
            {
                Result = result
            };
        }

        public async Task<ManagerBaseResponse<bool>> AddRegisterDevice(RegisterDeviceModel model, string userId)
        {
            try
            {
                var RegisterDevice = await unitOfWork.RegisterDeviceRepository
                                          .GetAsync(entity => !entity.IsDeleted && entity.UId == model.UId && entity.UserId == userId);
                if (RegisterDevice == null)
                {
                    await unitOfWork.RegisterDeviceRepository.AddAsync(new RegisterDeviceEntity()
                    {
                        UserId = userId,
                        DeviceToken = model.DeviceToken,
                        UId = model.UId,
                        DeviceType = model.DeviceType,
                        AppVersion = model.AppVersion,
                        OSVersion = model.OSVersion,
                        Lat = model.Latitude,
                        Long = model.Longitude,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        IsDeleted = false
                    });
                    await unitOfWork.CommitAsync();
                }
                else
                {
                    RegisterDevice.DeviceToken = model.DeviceToken;
                    RegisterDevice.DeviceType = model.DeviceType;
                    RegisterDevice.AppVersion = model.AppVersion;
                    RegisterDevice.OSVersion = model.OSVersion;
                    RegisterDevice.Lat = model.Latitude;
                    RegisterDevice.Long = model.Longitude;
                    RegisterDevice.UpdatedBy = userId;
                    RegisterDevice.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
                return new ManagerBaseResponse<bool>().Success("Device has been registered successfully", true);
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>().Failed(500, ex.Message.ToString(), false);
            }
        }

        public async Task<ManagerBaseResponse<bool>> DeleteRegisterDevice(string uid, string userId)
        {
            try
            {
                var RegisterDevices = await unitOfWork.RegisterDeviceRepository
                                            .GetQueryable(entity => !entity.IsDeleted && entity.UId == uid && entity.UserId == userId)
                                            .ToListAsync();
                if (RegisterDevices.Any())
                {
                    foreach (var RegisterDevice in RegisterDevices)
                    {
                        RegisterDevice.IsDeleted = true;
                        RegisterDevice.DeletedBy = userId;
                        RegisterDevice.DeletedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                    return new ManagerBaseResponse<bool>().Success("Device has been deleted successfully", true);
                }
                return new ManagerBaseResponse<bool>().Failed(500, "Device not found", false);
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>().Failed(500, ex.Message.ToString(), false);
            }
        }
    }
}