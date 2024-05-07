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
    public class AddressesManager : IAddressesManager
    {
        private readonly IUnitOfWork unitOfWork;
        public AddressesManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ManagerBaseResponse<IEnumerable<AddressResponseModel>>> Get(string userId)
        {
            var result = await unitOfWork
                .AddressesRepository
                .GetQueryable(x => !x.IsDeleted && x.CreatedBy == userId)
                .Select(x => new AddressResponseModel()
                {
                    Id = x.Id,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    CityName = x.CityName,
                    StateName = x.StateName,
                    CountryName = x.CountryName,
                    ZipCode = x.ZipCode,
                    TypeOfProperty = x.TypeOfProperty,
                    IsDefault = x.IsDefault,
                    IsSameMailingAddress = x.IsSameMailingAddress
                })
                .AsNoTracking()
                .ToListAsync();

            return new ManagerBaseResponse<IEnumerable<AddressResponseModel>>()
            {
                Result = result
            };
        }

        public async Task<ManagerBaseResponse<bool>> AddUpdateAddress(List<AddressesRequestModel> model, string userId)
        {
            try
            {
                var typeCount = model.Select(x => x.TypeOfProperty).Distinct().Count();
                if (typeCount == 1 && model.Count() == 2)
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "TypeOfProperty for both addresses are same.",
                        Result = false
                    };
                }

                foreach (var item in model)
                {
                    var entity = await unitOfWork
                        .AddressesRepository
                        .GetAsync(entity => !entity.IsDeleted && entity.CreatedBy == userId
                                            && entity.TypeOfProperty == item.TypeOfProperty);
                    if (entity == null)
                    {
                        await unitOfWork.AddressesRepository
                            .AddAsync(new AddressesEntity()
                            {
                                Id = item.Id,
                                AddressLine1 = item.AddressLine1,
                                AddressLine2 = item.AddressLine2,
                                CityName = item.CityName,
                                StateName = item.StateName,
                                CountryName = item.CountryName,
                                ZipCode = item.ZipCode,
                                TypeOfProperty = item.TypeOfProperty,
                                IsDefault = item.IsDefault,
                                IsSameMailingAddress = item.IsSameMailingAddress,
                                IsDeleted = false,
                                CreatedBy = userId,
                                CreatedOn = DateTime.UtcNow,
                                UpdatedBy = userId,
                                UpdatedOn = DateTime.UtcNow
                            });
                        await unitOfWork.CommitAsync();
                    }
                    else
                    {
                        entity.AddressLine1 = item.AddressLine1;
                        entity.AddressLine2 = item.AddressLine2;
                        entity.CityName = item.CityName;
                        entity.StateName = item.StateName;
                        entity.CountryName = item.CountryName;
                        entity.ZipCode = item.ZipCode;
                        entity.TypeOfProperty = item.TypeOfProperty;
                        entity.IsDefault = item.IsDefault;
                        entity.IsSameMailingAddress = item.IsSameMailingAddress;
                        entity.UpdatedBy = userId;
                        entity.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Address added/updated successfully.",
                    Result = true
                };

            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false
                };
            }
        }
    }
}
