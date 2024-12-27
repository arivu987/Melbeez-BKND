using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class LocationsManager : ILocationsManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> orderByTranslations = new();
        private readonly ISendNotificationManager sendNotificationManager;
        private readonly UserManager<ApplicationUser> userManager;
        public LocationsManager(IUnitOfWork unitOfWork, IConfiguration configuration, ISendNotificationManager sendNotificationManager, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.sendNotificationManager = sendNotificationManager;
            this.userManager = userManager;
        }

        public async Task<ManagerBaseResponse<IEnumerable<LocationsResponseModel>>> Get(string locationTypes, string categoryIds, DateTime? warrantyFrom, DateTime? warrantyTo, PagedListCriteria pagedListCriteria, string userId)
        {
            var mediaBaseUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            List<int> intlocationTypes = locationTypes?
                                       .Split(',')
                                       .Select(x => Convert.ToInt32(x))
                                       .ToList();

            List<long> intCategoryIds = categoryIds?
                                       .Split(',')
                                       .Select(x => Convert.ToInt64(x))
                                       .ToList();

            var result = await unitOfWork.LocationsRepository
                .GetQueryable(x => !x.IsDeleted && x.Status != MovedStatus.Transferred && x.CreatedBy == userId)
                .Include(u => u.ProductDetail)
                .Select(x => new LocationsResponseModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    CityName = x.CityName,
                    ZipCode = x.ZipCode,
                    StateName = x.StateName,
                    CountryName = x.CountryName,
                    Image = string.IsNullOrEmpty(x.Image) ? x.Image : mediaBaseUrl + x.Image,
                    TypeOfProperty = x.TypeOfProperty,
                    IsDefault = x.IsDefault,
                    CategoryId = x.ProductDetail
                                  .FirstOrDefault(a => a.LocationId == x.Id && !a.IsDeleted)
                                  .CategoryId,
                    ProductsCount = x.ProductDetail
                                    .Where(a => !a.IsDeleted && a.LocationId == x.Id
                                             && a.Status != MovedStatus.Transferred && a.CreatedBy == userId)
                                    .Count(),
                    Currency = x.ProductDetail
                                .FirstOrDefault(a => a.LocationId == x.Id && !a.IsDeleted)
                                .Currency,
                    TotalProductsAmount = !x.ProductDetail.Any() ? null : Convert.ToDouble(x.ProductDetail
                                           .Where(a => !a.IsDeleted && a.LocationId == x.Id
                                                    && a.Status != MovedStatus.Transferred && a.CreatedBy == userId)
                                           .Select(b => b.Price)
                                           .Sum().ToString("#.00")),
                    WarrantyFrom = x.ProductDetail
                                    .FirstOrDefault(a => a.LocationId == x.Id && !a.IsDeleted)
                                    .ProductWarrantiesDetails.FirstOrDefault(b => !b.IsDeleted)
                                    .StartDate,
                    WarrantyTo = x.ProductDetail
                                  .FirstOrDefault(a => a.LocationId == x.Id && !a.IsDeleted)
                                  .ProductWarrantiesDetails.FirstOrDefault(b => !b.IsDeleted)
                                  .EndDate,
                    UpdatedOn = x.UpdatedOn,
                    IsMoving = x.ProductDetail
                                .Where(a => a.LocationId == x.Id
                                        && !a.IsDeleted
                                        && (a.Status == MovedStatus.Initiated
                                            || a.Status == MovedStatus.Waiting)).Any() ? true : x.IsMoving,
                    Status = x.Status
                })
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.Name.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                     || x.AddressLine1.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                     || x.AddressLine2.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                     || x.CityName.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                     || x.StateName.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                     || x.CountryName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .WhereIf(!string.IsNullOrEmpty(locationTypes), x => intlocationTypes.Contains((int)x.TypeOfProperty))
                .WhereIf(!string.IsNullOrEmpty(categoryIds), x => intCategoryIds.Contains((long)x.CategoryId))
                .WhereIf(warrantyFrom.HasValue && !(warrantyTo.HasValue), w => w.WarrantyFrom.Value >= warrantyFrom.Value.ToUniversalTime())
                .WhereIf(warrantyTo.HasValue && !(warrantyFrom.HasValue), w => w.WarrantyTo.Value <= warrantyTo.Value.ToUniversalTime())
                .WhereIf(warrantyFrom.HasValue && warrantyTo.HasValue, w => w.WarrantyFrom.Value >= warrantyFrom.Value.ToUniversalTime() && w.WarrantyTo.Value <= warrantyTo.Value.ToUniversalTime())
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);

            foreach (var item in result.Data)
            {
                string toUserName = null;

                var locationTranserDetails = await unitOfWork.ItemTransferRepository
                                           .GetAsync(x => !x.IsDeleted && !x.IsProduct && x.Status != MovedStatus.Rejected
                                                       && x.Status != MovedStatus.Cancelled && x.Status != MovedStatus.Expired
                                                       && x.ItemId == item.Id);
                if (locationTranserDetails != null)
                {
                    var toUserData = userManager.Users.FirstOrDefault(x => !x.IsDeleted && x.Id == locationTranserDetails.ToUserId);
                    if (toUserData == null)
                    {
                        var itemTransferInvitation = await unitOfWork.ItemTransferInvitationRepository
                                                .GetAsync(x => x.TransferId == locationTranserDetails.TransferId);

                        toUserName = itemTransferInvitation.Email != ""
                                   ? itemTransferInvitation.Email
                                   : itemTransferInvitation.CountryCode + itemTransferInvitation.PhoneNumber;
                    }
                    else
                    {
                        toUserName = toUserData.FirstName + ' ' + toUserData.LastName;
                    }

                    item.TransferId = locationTranserDetails == null ? null : locationTranserDetails.TransferId;
                    item.TransferTo = toUserName;
                    item.TransferExpireOn = locationTranserDetails == null ? null : locationTranserDetails.ExpireOn;
                    item.TransferInitiatedOn = locationTranserDetails == null ? null : locationTranserDetails.CreatedOn;
                }
            }

            return new ManagerBaseResponse<IEnumerable<LocationsResponseModel>>()
            {
                Result = result.Data,
                PageDetail = new PageDetailModel()
                {
                    Skip = pagedListCriteria.Skip,
                    Take = pagedListCriteria.Take,
                    Count = result.TotalCount,
                    SearchText = pagedListCriteria.SearchText
                }
            };
        }
        public async Task<ManagerBaseResponse<LocationsResponseModel>> Get(long id, string userId)
        {
            var mediaBaseUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            string toUserName = null;

            var locationTranserDetails = await unitOfWork.ItemTransferRepository
                                       .GetAsync(x => !x.IsDeleted && x.Status != MovedStatus.Rejected
                                                   && x.Status != MovedStatus.Cancelled && !x.IsProduct
                                                   && x.ItemId == id);
            if (locationTranserDetails != null)
            {
                var toUserData = userManager.Users.FirstOrDefault(x => !x.IsDeleted && x.Id == locationTranserDetails.ToUserId);
                if (toUserData == null)
                {
                    var itemTransferInvitation = await unitOfWork.ItemTransferInvitationRepository
                                            .GetAsync(x => x.TransferId == locationTranserDetails.TransferId);

                    toUserName = itemTransferInvitation.Email != ""
                               ? itemTransferInvitation.Email
                               : itemTransferInvitation.CountryCode + itemTransferInvitation.PhoneNumber;
                }
                else
                {
                    toUserName = toUserData.FirstName + ' ' + toUserData.LastName;
                }
            }

            var result = await unitOfWork
                .LocationsRepository
                .GetQueryable(x => !x.IsDeleted && x.Id == id)
                .Include(u => u.ProductDetail)
                .Select(x => new LocationsResponseModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    CityName = x.CityName,
                    ZipCode = x.ZipCode,
                    StateName = x.StateName,
                    CountryName = x.CountryName,
                    Image = x.Image == null ? x.Image : mediaBaseUrl + x.Image,
                    TypeOfProperty = x.TypeOfProperty,
                    IsDefault = x.IsDefault,
                    IsMoving = x.ProductDetail.Where(a => a.LocationId == x.Id
                                                && !a.IsDeleted
                                                && (a.Status == MovedStatus.Initiated
                                                || a.Status == MovedStatus.Waiting)).Any() ? true : x.IsMoving,
                    Status = x.Status,
                    TransferId = locationTranserDetails == null ? null : locationTranserDetails.TransferId,
                    TransferTo = toUserName,
                    TransferExpireOn = locationTranserDetails == null ? null : locationTranserDetails.ExpireOn,
                    TransferInitiatedOn = locationTranserDetails == null ? null : locationTranserDetails.CreatedOn
                })
                .FirstOrDefaultAsync();

            return new ManagerBaseResponse<LocationsResponseModel>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<LocationsEntity>> AddLocation(LocationsRequestModel model, string userId)
        {
            try
            {
                if (model.Id == 0)
                {
                    var isExists = await IsLocationAlreadyExists(null, model.Name, userId);
                    if (isExists)
                    {
                        return new ManagerBaseResponse<LocationsEntity>()
                        {
                            Message = "Location name already exists",
                            Result = null,
                            StatusCode = 500
                        };
                    }

                    var response = await unitOfWork.LocationsRepository.AddAsync(new LocationsEntity()
                    {
                        Id = model.Id,
                        Name = model.Name,
                        AddressLine1 = model.AddressLine1,
                        AddressLine2 = model.AddressLine2,
                        CityName = model.CityName,
                        CountryName = model.CountryName,
                        StateName = model.StateName,
                        ZipCode = model.ZipCode,
                        Image = model.Image,
                        TypeOfProperty = model.TypeOfProperty,
                        IsDefault = model.IsDefault,
                        IsMoving = false,
                        Status = MovedStatus.None,
                        IsDeleted = false,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                    });
                    await unitOfWork.CommitAsync();

                    if (response != null && response.Id != 0 && model.IsDefault)
                    {
                        var oldEntity = await unitOfWork
                            .LocationsRepository
                            .GetAsync(oldEntity => oldEntity.Id != response.Id && oldEntity.CreatedBy == userId
                                                   && !oldEntity.IsDeleted && oldEntity.IsDefault);
                        if (oldEntity != null)
                        {
                            oldEntity.IsDefault = false;
                            oldEntity.UpdatedBy = userId;
                            oldEntity.UpdatedOn = DateTime.UtcNow;
                            await unitOfWork.CommitAsync();
                        }
                    }

                    await sendNotificationManager.SendLocationUpdateNotification(new PushNotificationRequestModel()
                    {
                        RecipientId = userId,
                        Title = "New Location has been added",
                        Description = "New Location " + model.Name + " has been added",
                        NotificationType = NotificationType.LocationUpdate,
                        ReferenceId = model.Id.ToString()
                    }, userId);

                    // Fetch the complete record from the database
                    var fullResponse = await unitOfWork.LocationsRepository.GetAsync(x => x.Id == response.Id);

                    return new ManagerBaseResponse<LocationsEntity>()
                    {
                        Message = "Location added successfully.",
                        Result = fullResponse
                    };
                }
                else
                {
                    return new ManagerBaseResponse<LocationsEntity>()
                    {
                        Message = "Invalid location request.",
                        Result = null,
                        StatusCode = 500
                    };
                }
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<LocationsEntity>()
                {
                    Message = ex.Message,
                    Result = null,
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateLocation(LocationsRequestModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                            .LocationsRepository
                            .GetAsync(entity => entity.Id == model.Id && entity.CreatedBy == userId && !entity.IsDeleted);

                if (entity != null)
                {
                    var isExists = await IsLocationAlreadyExists(model.Id, model.Name, userId);
                    if (isExists)
                    {
                        return new ManagerBaseResponse<bool>()
                        {
                            Message = "Location name already exists",
                            Result = false,
                            StatusCode = 500
                        };
                    }

                    entity.Name = model.Name;
                    entity.AddressLine1 = model.AddressLine1;
                    entity.AddressLine2 = model.AddressLine2;
                    entity.CityName = model.CityName;
                    entity.ZipCode = model.ZipCode;
                    entity.CountryName = model.CountryName;
                    entity.StateName = model.StateName;
                    entity.Image = model.Image;
                    entity.TypeOfProperty = model.TypeOfProperty;
                    entity.IsDefault = model.IsDefault;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;

                    await unitOfWork.CommitAsync();

                    if (model.IsDefault)
                    {
                        var oldEntity = await unitOfWork
                                       .LocationsRepository
                                       .GetAsync(oldEntity => oldEntity.Id != model.Id
                                                           && oldEntity.CreatedBy == userId
                                                           && !oldEntity.IsDeleted && oldEntity.IsDefault);
                        if (oldEntity != null)
                        {
                            oldEntity.IsDefault = false;
                            await unitOfWork.CommitAsync();
                        }
                    }
                }
                else
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Invalid location request.",
                        Result = false,
                        StatusCode = 500
                    };
                }

                await sendNotificationManager.SendLocationUpdateNotification(new PushNotificationRequestModel()
                {
                    RecipientId = userId,
                    Title = "Location has been updated",
                    Description = "Location details have been updated for the " + entity.Name + " location",
                    NotificationType = NotificationType.LocationUpdate,
                    ReferenceId = entity.Id.ToString()
                }, userId);

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Location updated successfully.",
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
        public async Task<ManagerBaseResponse<bool>> DeleteLocation(List<long> Id, string userId, string callingFrom = null)
        {
            try
            {
                foreach (var id in Id)
                {
                    var entity = await unitOfWork
                                .LocationsRepository
                                .GetAsync(entity => entity.Id == id
                                                 && entity.CreatedBy == userId
                                                 && !entity.IsDeleted);

                    if (entity != null)
                    {
                        #region Get Associated Products and updated IsDeleted as true.

                        var products = await unitOfWork
                                      .ProductsRepository
                                      .GetListAsync(p => !p.IsDeleted && p.LocationId == entity.Id && p.CreatedBy == userId);

                        if (products.Any())
                        {
                            foreach (var product in products)
                            {
                                #region Get Associated Product Images and updated IsDeleted as true.

                                var productImages = await unitOfWork
                                               .ProductImageRepository
                                               .GetListAsync(i => !i.IsDeleted && i.ProductId == product.Id && i.CreatedBy == userId);

                                if (productImages.Any())
                                {
                                    foreach (var productImage in productImages)
                                    {
                                        productImage.IsDeleted = true;
                                        productImage.DeletedBy = userId;
                                        productImage.DeletedOn = DateTime.UtcNow;
                                        await unitOfWork.CommitAsync();
                                    }
                                }

                                #endregion

                                #region Get Associated Product Warranties and updated IsDeleted as true.

                                var productWarranties = await unitOfWork
                                                       .ProductWarrantiesRepository
                                                       .GetListAsync(w => !w.IsDeleted && w.ProductId == product.Id && w.CreatedBy == userId);

                                if (productWarranties.Any())
                                {
                                    foreach (var productWarranty in productWarranties)
                                    {
                                        productWarranty.IsDeleted = true;
                                        productWarranty.DeletedBy = userId;
                                        productWarranty.DeletedOn = DateTime.UtcNow;
                                        await unitOfWork.CommitAsync();
                                    }
                                }

                                #endregion

                                #region Get Related Receiptes and updated IsDeleted as true

                                var receiptsList = await unitOfWork
                                                  .ReceiptProductRepository
                                                  .GetListAsync(w => !w.IsDeleted && w.ProductId == product.Id && w.CreatedBy == userId);

                                if (receiptsList.Any())
                                {
                                    foreach (var receipt in receiptsList)
                                    {
                                        receipt.IsDeleted = true;
                                        receipt.DeletedBy = userId;
                                        receipt.DeletedOn = DateTime.UtcNow;
                                        await unitOfWork.CommitAsync();
                                    }
                                }

                                #endregion

                                product.IsDeleted = true;
                                product.DeletedBy = userId;
                                product.DeletedOn = DateTime.UtcNow;
                                await unitOfWork.CommitAsync();
                            }
                        }

                        #endregion

                        entity.IsDeleted = true;
                        entity.DeletedBy = userId;
                        entity.DeletedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }

                    if (callingFrom != "DeleteAccount")
                    {
                        await sendNotificationManager.SendLocationUpdateNotification(new PushNotificationRequestModel()
                        {
                            RecipientId = userId,
                            Title = "Location has been deleted",
                            Description = "The location " + entity.Name + " has been deleted",
                            NotificationType = NotificationType.LocationUpdate,
                            ReferenceId = entity.Id.ToString()
                        }, userId);
                    }
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Location(s) deleted successfully.",
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
        private async Task<bool> IsLocationAlreadyExists(long? id, string locationName, string userId)
        {
            bool isAlreadyExists = await unitOfWork
                              .LocationsRepository
                              .GetQueryable(x => !x.IsDeleted && x.CreatedBy == userId
                                              && x.Name.ToLower().Equals(locationName.ToLower())
                                              && x.Status != MovedStatus.Transferred)
                              .WhereIf(id != null, x => x.Id != id)
                              .AnyAsync();
            return isAlreadyExists;
        }
        public async Task<ManagerBaseResponse<IEnumerable<LocationsResponseModel>>> GetLocationByUserId(string userId)
        {
            var mediaBaseUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var result = await unitOfWork
                .LocationsRepository
                .GetQueryable(x => !x.IsDeleted && x.CreatedBy == userId)
                .Select(x => new LocationsResponseModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    AddressLine1 = x.AddressLine1,
                    AddressLine2 = x.AddressLine2,
                    CityName = x.CityName,
                    ZipCode = x.ZipCode,
                    StateName = x.StateName,
                    CountryName = x.CountryName,
                    Image = x.Image == null ? x.Image : mediaBaseUrl + x.Image,
                    TypeOfProperty = x.TypeOfProperty,
                    IsDefault = x.IsDefault,
                    IsMoving = x.IsMoving,
                    Status = x.Status
                })
                .ToListAsync();

            return new ManagerBaseResponse<IEnumerable<LocationsResponseModel>>()
            {
                Result = result
            };
        }
    }
}
