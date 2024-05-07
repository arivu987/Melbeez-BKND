using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ItemTransferManager : IItemTransferManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly ISendNotificationManager sendNotificationManager;
        private readonly UserManager<ApplicationUser> userManager;

        public ItemTransferManager(IUnitOfWork unitOfWork,
                               IConfiguration configuration,
                               ISendNotificationManager sendNotificationManager,
                               UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.sendNotificationManager = sendNotificationManager;
            this.userManager = userManager;
        }
        public async Task<ManagerBaseResponse<List<TransferItemResponse>>> GetTransferItem(string userId, bool isRecevier, MovedStatus? status)
        {
            try
            {
                List<TransferItemResponse> transferItemResponseslst = new List<TransferItemResponse>();
                var mediaBaseUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");

                var movedStatus = new List<MovedStatus>();
                movedStatus.Add(MovedStatus.Initiated);
                movedStatus.Add(MovedStatus.Waiting);
                movedStatus.Add(MovedStatus.Transferred);
                movedStatus.Add(MovedStatus.Cancelled);
                movedStatus.Add(MovedStatus.Rejected);
                movedStatus.Add(MovedStatus.Expired);

                var itemTransferlst = await unitOfWork.ItemTransferRepository
                                    .GetQueryable(x => !x.IsDeleted)
                                    .WhereIf(isRecevier, x => x.ToUserId == userId)
                                    .WhereIf(!isRecevier, x => x.FromUserId == userId)
                                    .WhereIf(status.HasValue, x => x.Status == status.Value)
                                    .WhereIf(isRecevier && !status.HasValue, x => movedStatus.Contains(x.Status))
                                    .ToListAsync();

                var response = itemTransferlst.GroupBy(g => new
                {
                    TransferId = g.TransferId,
                    IsProduct = g.IsProduct,
                    FromUserId = g.FromUserId,
                    ToUserId = g.ToUserId,
                    Status = g.Status,
                })
                .Select(x => new
                {
                    TransferId = x.Key.TransferId,
                    IsProduct = x.Key.IsProduct,
                    FromUserId = x.Key.FromUserId,
                    ToUserId = x.Key.ToUserId,
                    Status = x.Key.Status
                })
                .ToList();

                foreach (var item in response)
                {
                    var itemIds = itemTransferlst.Where(x => x.TransferId == item.TransferId)
                                .Select(x => new { x.ItemId, x.DependentProductIds })
                                .Distinct()
                                .ToList();

                    var itemtransferedUsers = new List<string> { item.ToUserId, item.FromUserId };
                    var transferUsersInfo = userManager.Users.Where(x => itemtransferedUsers.Contains(x.Id)).ToList();

                    var fromUserInfo = transferUsersInfo.FirstOrDefault(x => x.Id == item.FromUserId);
                    var toUserInfo = transferUsersInfo.FirstOrDefault(x => x.Id == item.ToUserId);

                    string toUserName = toUserInfo != null && toUserInfo.IsDeleted
                        ? "Deleted User"
                        : toUserInfo == null
                            ? GetUserInfo(item.TransferId)
                        : toUserInfo.FirstName + ' ' + toUserInfo.LastName;

                    string fromUserName = fromUserInfo != null && fromUserInfo.IsDeleted
                        ? "Deleted User"
                        : fromUserInfo?.FirstName + ' ' + fromUserInfo?.LastName;

                    if (string.IsNullOrEmpty(toUserName))
                    {
                        continue;
                    }

                    TransferItemResponse transferItemResponse = new TransferItemResponse();
                    transferItemResponse.TransferId = item.TransferId;
                    transferItemResponse.IsProduct = item.IsProduct;
                    transferItemResponse.FromUserId = item.FromUserId;
                    transferItemResponse.FromUserName = fromUserName;
                    transferItemResponse.ToUserId = item.ToUserId;
                    transferItemResponse.ToUserName = toUserName;
                    transferItemResponse.Status = item.Status;
                    transferItemResponse.ExpireOn = itemTransferlst.Where(x => x.TransferId == item.TransferId).Select(x => x.ExpireOn).FirstOrDefault();
                    transferItemResponse.CreatedOn = itemTransferlst.Where(x => x.TransferId == item.TransferId).Select(x => x.CreatedOn).FirstOrDefault();

                    var itemMovedStatus = new List<MovedStatus>();
                    itemMovedStatus.Add(MovedStatus.None);
                    itemMovedStatus.Add(MovedStatus.Cancelled);
                    itemMovedStatus.Add(MovedStatus.Rejected);

                    if (item.IsProduct)
                    {
                        var productlst = await unitOfWork.ProductsRepository
                                    .GetQueryable(x => itemIds.Select(i => i.ItemId).Contains(x.Id))
                                    .Include(u => u.ProductCategoriesDetail)
                                    .Include(u => u.LocationsDetail)
                                    .Include(u => u.ProductImageDetails)
                                    .Include(u => u.ProductWarrantiesDetails)
                                    .Select(s => new ProductsResponseModel()
                                    {
                                        ProductId = s.Id,
                                        ModelNumber = s.ModelNumber,
                                        ManufacturerName = s.ManufactureName,
                                        SerialNumber = s.SerialNumber,
                                        VendorName = s.VendorName,
                                        Notes = s.Notes,
                                        ProductName = s.Name,
                                        LocationId = s.LocationId,
                                        Location = s.LocationsDetail.Name,
                                        CategoryId = s.CategoryId,
                                        CategoryName = s.ProductCategoriesDetail.Name,
                                        PurchaseDate = s.PurchaseDate,
                                        Currency = s.Currency,
                                        Price = Convert.ToDouble(s.Price.ToString("#.00")),
                                        DefaultImageUrl = s.ProductImageDetails
                                                           .FirstOrDefault(x => !x.IsDeleted && x.ProductId == s.Id && x.IsDefault == true)
                                                           .ImageUrl,
                                        WarrantyFrom = s.ProductWarrantiesDetails
                                                        .FirstOrDefault(x => !x.IsDeleted && x.ProductId == s.Id && x.IsProduct)
                                                        .StartDate.Date,
                                        WarrantyTo = s.ProductWarrantiesDetails
                                                      .FirstOrDefault(x => !x.IsDeleted && x.ProductId == s.Id && x.IsProduct)
                                                      .EndDate.Date,
                                        BarCodeNumber = s.BarCodeNumber,
                                        CreatedOn = s.CreatedOn,
                                        IsMoving = s.IsMoving,
                                        Status = s.Status
                                    })
                                    .ToListAsync();

                        foreach (var product in productlst.Where(w => w.DefaultImageUrl != null))
                        {
                            product.DefaultImageUrl = Path.Combine(mediaBaseUrl, product.DefaultImageUrl);
                        }
                        transferItemResponse.ProductList = productlst;
                        transferItemResponse.LocationList = new List<LocationTransferResponseModel>();
                        transferItemResponseslst.Add(transferItemResponse);
                    }
                    else
                    {
                        var dependentStingProductIds = itemIds.Select(q => q.DependentProductIds).ToList();
                        var dependentProductIds = new List<long>();
                        foreach (var dependentString in dependentStingProductIds)
                        {
                            if (dependentString.Contains(","))
                            {
                                var abc = dependentString.Split(",");
                                dependentProductIds = abc.Select(x => long.TryParse(x, out var result) ? result : 0).ToList();
                            }
                            else
                            {
                                dependentProductIds.Add(Convert.ToInt64(dependentString));
                            }

                        }
                        if (isRecevier)
                        {
                            var locationlst = await unitOfWork.LocationsRepository
                            .GetQueryable(x => itemIds.Select(i => i.ItemId).Contains(x.Id))
                            .Include(u => u.ProductDetail)
                            .Select(x => new LocationTransferResponseModel()
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
                                              .FirstOrDefault(a => a.LocationId == x.Id)
                                              .CategoryId,
                                Products = x.ProductDetail.Where(a => a.LocationId == x.Id
                                                                   && dependentProductIds.Contains(a.Id))
                                        .Select(ps => new ProductsTransferResponseModel()
                                        {
                                            ProductId = ps.Id,
                                            ProductName = ps.Name,
                                            ProductTitle = ps.Title,
                                            PurchaseDate = ps.PurchaseDate,
                                            Currency = ps.Currency,
                                            Price = ps.Price,
                                            DefaultImageUrl = string.IsNullOrEmpty(ps.ProductImageDetails
                                                            .FirstOrDefault(pi => !pi.IsDeleted
                                                                               && pi.ProductId == ps.Id
                                                                               && pi.IsDefault == true)
                                                            .ImageUrl) ? null : string.Concat(mediaBaseUrl, ps.ProductImageDetails
                                                            .FirstOrDefault(pi => !pi.IsDeleted
                                                                               && pi.ProductId == ps.Id
                                                                               && pi.IsDefault == true)
                                                            .ImageUrl)
                                        }).ToList(),
                                ProductsCount = x.ProductDetail
                                                .Where(a => a.LocationId == x.Id
                                                             && dependentProductIds.Contains(a.Id))
                                                .Count(),
                                Currency = x.ProductDetail
                                            .FirstOrDefault(a => a.LocationId == x.Id)
                                            .Currency,
                                TotalProductsAmount = !x.ProductDetail.Any() ? null : Convert.ToDouble(x.ProductDetail
                                                       .Where(a => a.LocationId == x.Id
                                                                && dependentProductIds.Contains(a.Id))
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
                                IsMoving = x.IsMoving,
                                Status = x.Status
                            }).ToListAsync();
                            transferItemResponse.LocationList = locationlst;
                        }
                        else
                        {
                            var locationlst = await unitOfWork.LocationsRepository
                            .GetQueryable(x => itemIds.Select(i => i.ItemId).Contains(x.Id))
                            .Include(u => u.ProductDetail)
                            .Select(x => new LocationTransferResponseModel()
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
                                              .FirstOrDefault(a => a.LocationId == x.Id)
                                              .CategoryId,
                                Products = x.ProductDetail.Where(a => a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
                                        .Select(ps => new ProductsTransferResponseModel()
                                        {
                                            ProductId = ps.Id,
                                            ProductName = ps.Name,
                                            ProductTitle = ps.Title,
                                            PurchaseDate = ps.PurchaseDate,
                                            Currency = ps.Currency,
                                            Price = ps.Price,
                                            DefaultImageUrl = string.IsNullOrEmpty(ps.ProductImageDetails
                                                            .FirstOrDefault(pi => !pi.IsDeleted
                                                                               && pi.ProductId == ps.Id
                                                                               && pi.IsDefault == true)
                                                            .ImageUrl) ? null : string.Concat(mediaBaseUrl, ps.ProductImageDetails
                                                            .FirstOrDefault(pi => !pi.IsDeleted
                                                                               && pi.ProductId == ps.Id
                                                                               && pi.IsDefault == true)
                                                            .ImageUrl)
                                        }).ToList(),
                                ProductsCount = x.ProductDetail
                                                .Where(a => a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
                                                .Count(),
                                Currency = x.ProductDetail
                                            .FirstOrDefault(a => a.LocationId == x.Id && !a.IsDeleted)
                                            .Currency,
                                TotalProductsAmount = !x.ProductDetail.Any() ? null : Convert.ToDouble(x.ProductDetail
                                                       .Where(a => a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
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
                                IsMoving = x.IsMoving,
                                Status = x.Status
                            }).ToListAsync();
                            transferItemResponse.LocationList = locationlst;
                        }
                        transferItemResponse.ProductList = new List<ProductsResponseModel>();
                        transferItemResponseslst.Add(transferItemResponse);
                    }
                }
                return new ManagerBaseResponse<List<TransferItemResponse>>()
                {
                    Result = transferItemResponseslst.OrderByDescending(x => x.CreatedOn).ToList(),
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<List<TransferItemResponse>>()
                {
                    Message = ex.Message,
                    Result = new List<TransferItemResponse>(),
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> TransferItem(ItemTransferRequestModel model, string userId)
        {
            try
            {
                Random random = new Random();
                var transferId = random.Next(100000000, 999999999);
                string transferIdString = transferId.ToString("D9");
                bool isItemExists = false;

                foreach (var item in model.ItemIds)
                {
                    if (model.IsProduct)
                    {
                        var productInfo = await unitOfWork.ProductsRepository
                                  .GetQueryable(x => !x.IsDeleted && x.Id == item && x.CreatedBy == userId
                                                && x.Status != MovedStatus.Transferred
                                                && x.Status != MovedStatus.Initiated
                                                && x.Status != MovedStatus.Waiting)
                                  .FirstOrDefaultAsync();

                        if (productInfo != null)
                        {
                            await unitOfWork.ItemTransferRepository
                            .AddAsync(new MovedItemStatusTransactonsEntity()
                            {
                                TransferId = transferIdString,
                                ItemId = item,
                                FromUserId = userId,
                                ToUserId = model.TransferTo,
                                Status = MovedStatus.Waiting,
                                IsProduct = model.IsProduct,
                                ExpireOn = DateTime.UtcNow.AddMonths(1).Date,
                                IsDeleted = false,
                                CreatedBy = userId,
                                CreatedOn = DateTime.UtcNow,
                                UpdatedBy = userId,
                                UpdatedOn = DateTime.UtcNow
                            });
                            await unitOfWork.CommitAsync();

                            productInfo.IsMoving = true;
                            productInfo.Status = MovedStatus.Waiting;
                            await unitOfWork.CommitAsync();
                            isItemExists = true;
                        }
                    }
                    else
                    {
                        var locationInfo = await unitOfWork.LocationsRepository
                                .GetQueryable(x => !x.IsDeleted && x.Id == item
                                                && x.Status != MovedStatus.Transferred
                                                && x.Status != MovedStatus.Initiated
                                                && x.Status != MovedStatus.Waiting
                                                && x.CreatedBy == userId)
                                .FirstOrDefaultAsync();

                        if (locationInfo != null)
                        {
                            var productInfoList = await unitOfWork.ProductsRepository
                                .GetQueryable(x => !x.IsDeleted && x.LocationId == locationInfo.Id
                                                && x.Status != MovedStatus.Transferred
                                                && x.Status != MovedStatus.Initiated
                                                && x.Status != MovedStatus.Waiting
                                                && x.CreatedBy == userId)
                                .ToListAsync();

                            var productIdList = string.Empty;
                            if (productInfoList.Any())
                            {
                                productIdList = string.Join(",", productInfoList.Select(p => p.Id.ToString()));
                            }

                            await unitOfWork.ItemTransferRepository
                            .AddAsync(new MovedItemStatusTransactonsEntity()
                            {
                                TransferId = transferIdString,
                                ItemId = item,
                                FromUserId = userId,
                                ToUserId = model.TransferTo,
                                Status = MovedStatus.Waiting,
                                IsProduct = model.IsProduct,
                                ExpireOn = DateTime.UtcNow.AddMonths(1),
                                DependentProductIds = productIdList,
                                IsDeleted = false,
                                CreatedBy = userId,
                                CreatedOn = DateTime.UtcNow,
                                UpdatedBy = userId,
                                UpdatedOn = DateTime.UtcNow
                            });
                            await unitOfWork.CommitAsync();

                            locationInfo.TransferTo = model.TransferTo;
                            locationInfo.IsMoving = true;
                            locationInfo.Status = MovedStatus.Waiting;
                            await unitOfWork.CommitAsync();

                            if (productInfoList.Any())
                            {
                                foreach (var productInfo in productInfoList)
                                {
                                    productInfo.TransferTo = model.TransferTo;
                                    productInfo.IsMoving = true;
                                    productInfo.Status = MovedStatus.Waiting;
                                    await unitOfWork.CommitAsync();
                                }
                            }
                            isItemExists = true;
                        }
                    }
                }

                if (isItemExists)
                {
                    var userData = userManager.Users
                           .Where(x => !x.IsDeleted && x.Id == userId)
                           .FirstOrDefault();
                    var senderName = string.Concat(userData.FirstName, " ", userData.LastName);
                    var transferItem = model.IsProduct ? "Product transfer" : "Location transfer";
                    await sendNotificationManager.SendItemTransferNotification(new PushNotificationRequestModel()
                    {
                        RecipientId = model.TransferTo,
                        Title = "Item transfer request from " + senderName,
                        Description = "The Melbeez user " + senderName + " wants to transfer " + transferItem + " information with you.",
                        NotificationType = NotificationType.ItemMove,
                        ReferenceId = transferIdString,
                        Status = MovedStatus.Waiting
                    }, userId);

                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Item transfer request sent successfully.",
                        Result = true
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Item already in transfer state",
                    Result = false
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
        public async Task<ManagerBaseResponse<bool>> CancelOrRejectTransferItem(string transferId, MovedStatus movedStatus, string userId)
        {
            try
            {
                var userData = userManager.Users
                              .Where(x => !x.IsDeleted && x.Id == userId)
                              .FirstOrDefault();

                var itemTransferlst = await unitOfWork.ItemTransferRepository
                                    .GetListAsync(x => x.TransferId == transferId);

                if (itemTransferlst != null)
                {
                    foreach (var itemTransfer in itemTransferlst)
                    {
                        itemTransfer.Status = movedStatus;
                        itemTransfer.UpdatedBy = userId;
                        itemTransfer.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();

                        if (itemTransfer.IsProduct)
                        {
                            var productInfo = await unitOfWork.ProductsRepository
                                            .GetAsync(x => !x.IsDeleted && x.Id == itemTransfer.ItemId);
                            if (productInfo != null)
                            {
                                productInfo.TransferTo = null;
                                productInfo.IsMoving = false;
                                productInfo.Status = MovedStatus.None;
                                await unitOfWork.CommitAsync();

                                var senderName = string.Concat(userData.FirstName, " ", userData.LastName);
                                var title = movedStatus == MovedStatus.Cancelled ? "Product transfer request cancelled" : "Product transfer request rejeted";
                                var description = movedStatus == MovedStatus.Cancelled
                                                    ? senderName + " cancelled transfer request for '" + productInfo.Name + "' product to you."
                                                    : senderName + " rejected transfer request for '" + productInfo.Name + "' product to you.";
                                await sendNotificationManager.SendItemTransferNotification(new PushNotificationRequestModel()
                                {
                                    RecipientId = movedStatus == MovedStatus.Rejected ? itemTransfer.ToUserId : itemTransfer.FromUserId,
                                    Title = title,
                                    Description = description,
                                    NotificationType = NotificationType.ItemMove,
                                    ReferenceId = transferId,
                                    Status = movedStatus
                                }, userId);
                            }
                        }
                        else
                        {
                            var locationInfo = await unitOfWork.LocationsRepository
                                            .GetAsync(x => !x.IsDeleted && x.Id == itemTransfer.ItemId);

                            if (locationInfo != null)
                            {
                                locationInfo.TransferTo = null;
                                locationInfo.IsMoving = false;
                                locationInfo.Status = MovedStatus.None;
                                await unitOfWork.CommitAsync();

                                var productInfoList = await unitOfWork.ProductsRepository
                                                    .GetQueryable(x => !x.IsDeleted && x.LocationId == locationInfo.Id
                                                                    && itemTransfer.DependentProductIds.Contains(x.Id.ToString()))
                                                    .ToListAsync();

                                if (productInfoList.Any())
                                {
                                    foreach (var productInfo in productInfoList)
                                    {
                                        productInfo.TransferTo = null;
                                        productInfo.IsMoving = false;
                                        productInfo.Status = MovedStatus.None;
                                        await unitOfWork.CommitAsync();
                                    }
                                }

                                var senderName = string.Concat(userData.FirstName, " ", userData.LastName);
                                var title = movedStatus == MovedStatus.Cancelled ? "Location transfer request cancelled" : "Location transfer request rejeted";
                                var description = movedStatus == MovedStatus.Cancelled
                                                    ? senderName + " cancelled transfer request for '" + locationInfo.Name + "' location to you."
                                                    : senderName + " rejected transfer request for '" + locationInfo.Name + "' location to you.";

                                await sendNotificationManager.SendItemTransferNotification(new PushNotificationRequestModel()
                                {
                                    RecipientId = movedStatus == MovedStatus.Rejected ? userId : itemTransfer.FromUserId,
                                    Title = title,
                                    Description = description,
                                    NotificationType = NotificationType.ItemMove,
                                    ReferenceId = transferId,
                                    Status = movedStatus
                                }, userId);
                            }
                        }
                    }
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Item transfer request cancel or Rejected successfully.",
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
        public async Task<ManagerBaseResponse<bool>> ApproveTransferItem(string transferId, string userId, bool IsSameLocation, long? locationId)
        {
            try
            {
                var itemTransfers = await unitOfWork.ItemTransferRepository
                                    .GetQueryable(x => x.TransferId == transferId)
                                    .ToListAsync();

                foreach (var item in itemTransfers)
                {
                    if (item.IsProduct)
                    {
                        var product = await unitOfWork.ProductsRepository
                            .GetAsync(x => !x.IsDeleted && x.Id == item.ItemId
                                        && x.Status != MovedStatus.Transferred);
                        await AddProduct(product, IsSameLocation ? product.LocationId : locationId, userId);
                    }
                    else
                    {
                        var locationProductIds = unitOfWork.ProductsRepository
                                                   .GetQueryable(x => !x.IsDeleted && x.LocationId == item.ItemId
                                                                   && x.Status != MovedStatus.Transferred
                                                                   && x.TransferTo == userId)
                                                   .Select(x => x.Id)
                                                   .ToList();

                        var location = await unitOfWork.LocationsRepository
                            .GetAsync(x => !x.IsDeleted && x.Id == item.ItemId
                                        && x.Status != MovedStatus.Transferred && x.TransferTo == userId);

                        if (IsSameLocation)
                        {
                            var isDefaultLocationExists = await IsDefaultLocationExists(userId);
                            var existLocationCount = await LocationNameExistsCount(location.Name, userId);
                            var locationName = location.Name;
                            if (existLocationCount > 0)
                            {
                                locationName = string.Concat(location.Name, " (" + existLocationCount + ")");
                            }
                            var response = await unitOfWork.LocationsRepository
                                .AddAsync(new LocationsEntity()
                                {
                                    Name = locationName,
                                    AddressLine1 = location.AddressLine1,
                                    AddressLine2 = location.AddressLine2,
                                    CityName = location.CityName,
                                    ZipCode = location.ZipCode,
                                    CountryName = location.CountryName,
                                    StateName = location.StateName,
                                    Image = location.Image,
                                    TypeOfProperty = location.TypeOfProperty,
                                    IsDefault = isDefaultLocationExists ? false : location.IsDefault,
                                    IsMoving = false,
                                    Status = MovedStatus.None,
                                    IsDeleted = false,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow,
                                });
                            await unitOfWork.CommitAsync();

                            if (response != null && response.Id != 0)
                            {
                                foreach (var productId in locationProductIds)
                                {
                                    var product = await unitOfWork.ProductsRepository
                                           .GetAsync(x => !x.IsDeleted && x.Id == productId);
                                    await AddProduct(product, response.Id, userId);
                                }
                            }
                        }
                        else
                        {
                            var differlocation = await unitOfWork.LocationsRepository
                                .GetAsync(x => !x.IsDeleted && x.Id == locationId && x.Status != MovedStatus.Transferred);
                            foreach (var productId in locationProductIds)
                            {
                                var product = await unitOfWork.ProductsRepository
                                    .GetAsync(x => !x.IsDeleted && x.Id == productId);
                                await AddProduct(product, differlocation.Id, userId);
                            }
                        }

                        location.IsMoving = true;
                        location.Status = MovedStatus.Transferred;
                        location.UpdatedBy = userId;
                        location.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                    item.Status = MovedStatus.Transferred;
                    item.UpdatedBy = userId;
                    item.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }

                var itemTransfer = itemTransfers.FirstOrDefault();

                await sendNotificationManager.SendItemTransferNotification(new PushNotificationRequestModel()
                {
                    RecipientId = userId,
                    Title = itemTransfer.IsProduct ? "Product transfer request approved" : "Location transfer request approved",
                    Description = itemTransfer.IsProduct
                                    ? "The Product transfer request [Transfer Id - " + itemTransfer.TransferId + "] has been approved By receiver."
                                    : "The Location transfer request [Transfer Id - " + itemTransfer.TransferId + "] has been approved By receiver.",
                    NotificationType = NotificationType.ItemMove,
                    ReferenceId = transferId
                }, userId);

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Item transfer request approved successfully.",
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
        public async Task<ManagerBaseResponse<bool>> DeleteTransferItem(string transferId, string userId)
        {
            try
            {
                var itemTransfers = await unitOfWork.ItemTransferRepository
                                    .GetQueryable(x => !x.IsDeleted && x.TransferId == transferId)
                                    .ToListAsync();

                if (itemTransfers.Any())
                {
                    foreach (var item in itemTransfers)
                    {
                        if (item.IsProduct)
                        {
                            await DeleteProduct(item.ItemId, userId);
                        }
                        else
                        {
                            var location = await unitOfWork.LocationsRepository
                                        .GetAsync(p => !p.IsDeleted && p.Id == item.ItemId && p.CreatedBy == userId);
                            if (location != null)
                            {
                                var products = await unitOfWork.ProductsRepository
                                        .GetListAsync(p => !p.IsDeleted && p.LocationId == location.Id && p.CreatedBy == userId);

                                if (products.Any())
                                {
                                    foreach (var product in products)
                                    {
                                        await DeleteProduct(product.Id, userId);
                                    }
                                }

                                location.IsDeleted = true;
                                location.DeletedBy = userId;
                                location.DeletedOn = DateTime.UtcNow;
                                await unitOfWork.CommitAsync();
                            }
                        }

                        item.IsDeleted = true;
                        item.DeletedBy = userId;
                        item.DeletedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Item transfer request delete successfully.",
                        Result = true
                    };
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Item transfer id doesn't match our records.",
                    Result = false
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
        public async Task<ManagerBaseResponse<List<ItemTransferedHistoryResponse>>> GetTransferedItems(string userId)
        {
            List<ItemTransferedHistoryResponse> transferItemResponseslst = new List<ItemTransferedHistoryResponse>();
            try
            {
                var mediaBaseUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
                var itemTransferlst = await unitOfWork.ItemTransferRepository
                                    .GetQueryable(x => !x.IsDeleted && x.FromUserId == userId && x.Status == MovedStatus.Transferred)
                                    .ToListAsync();
                var response = itemTransferlst.GroupBy(g => new
                {
                    TransferId = g.TransferId,
                    IsProduct = g.IsProduct,
                    FromUserId = g.FromUserId,
                    ToUserId = g.ToUserId,
                    Status = g.Status,
                })
                .Select(x => new
                {
                    TransferId = x.Key.TransferId,
                    IsProduct = x.Key.IsProduct,
                    FromUserId = x.Key.FromUserId,
                    ToUserId = x.Key.ToUserId,
                    Status = x.Key.Status
                })
                .ToList();

                if (response.Any())
                {
                    foreach (var item in response)
                    {
                        ItemTransferedHistoryResponse transferItemResponse = new ItemTransferedHistoryResponse();
                        var itemIds = itemTransferlst.Where(x => x.TransferId == item.TransferId)
                               .Select(x => new { x.ItemId, x.DependentProductIds })
                               .Distinct()
                               .ToList();

                        if (item.IsProduct)
                        {
                            var productlst = await unitOfWork.ProductsRepository
                                .GetQueryable(x => !x.IsDeleted && itemIds.Select(i => i.ItemId).Contains(x.Id))
                                .Include(u => u.ProductCategoriesDetail)
                                .Include(u => u.LocationsDetail)
                                .Include(u => u.ProductImageDetails)
                                .Include(u => u.ProductWarrantiesDetails)
                                .Select(s => new ProductsResponseModel()
                                {
                                    ProductId = s.Id,
                                    ModelNumber = s.ModelNumber,
                                    ManufacturerName = s.ManufactureName,
                                    SerialNumber = s.SerialNumber,
                                    VendorName = s.VendorName,
                                    Notes = s.Notes,
                                    ProductName = s.Name,
                                    LocationId = s.LocationId,
                                    Location = s.LocationsDetail.Name,
                                    CategoryId = s.CategoryId,
                                    CategoryName = s.ProductCategoriesDetail.Name,
                                    PurchaseDate = s.PurchaseDate,
                                    Currency = s.Currency,
                                    Price = Convert.ToDouble(s.Price.ToString("#.00")),
                                    DefaultImageUrl = s.ProductImageDetails
                                                       .FirstOrDefault(x => !x.IsDeleted && x.ProductId == s.Id && x.IsDefault == true)
                                                       .ImageUrl,
                                    WarrantyFrom = s.ProductWarrantiesDetails
                                                    .FirstOrDefault(x => !x.IsDeleted && x.ProductId == s.Id && x.IsProduct)
                                                    .StartDate.Date,
                                    WarrantyTo = s.ProductWarrantiesDetails
                                                  .FirstOrDefault(x => !x.IsDeleted && x.ProductId == s.Id && x.IsProduct)
                                                  .EndDate.Date,
                                    BarCodeNumber = s.BarCodeNumber,
                                    CreatedOn = s.CreatedOn,
                                    IsMoving = s.IsMoving,
                                    Status = s.Status
                                })
                                .ToListAsync();

                            foreach (var product in productlst.Where(w => w.DefaultImageUrl != null))
                            {
                                product.DefaultImageUrl = Path.Combine(mediaBaseUrl, product.DefaultImageUrl);
                            }
                            transferItemResponse.IsProduct = true;
                            transferItemResponse.Products = productlst;
                            transferItemResponse.Locations = new List<LocationTransferResponseModel>();
                            transferItemResponseslst.Add(transferItemResponse);
                        }
                        else
                        {
                            var dependentStingProductIds = itemIds.Select(q => q.DependentProductIds).ToList();
                            var dependentProductIds = new List<long>();
                            foreach (var dependentString in dependentStingProductIds)
                            {
                                if (dependentString.Contains(","))
                                {
                                    var abc = dependentString.Split(",");
                                    dependentProductIds = abc.Select(x => long.TryParse(x, out var result) ? result : 0).ToList();
                                }
                                else
                                {
                                    dependentProductIds.Add(Convert.ToInt64(dependentString));
                                }
                            }

                            var locationlst = await unitOfWork.LocationsRepository
                                .GetQueryable(x => !x.IsDeleted && itemIds.Select(i => i.ItemId).Contains(x.Id))
                                .Include(u => u.ProductDetail)
                                .Select(x => new LocationTransferResponseModel(){
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
                                                  .FirstOrDefault(a => a.LocationId == x.Id)
                                                  .CategoryId,
                                    Products = x.ProductDetail.Where(a => a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
                                                .Select(ps => new ProductsTransferResponseModel()
                                                {
                                                    ProductId = ps.Id,
                                                    ProductName = ps.Name,
                                                    ProductTitle = ps.Title,
                                                    PurchaseDate = ps.PurchaseDate,
                                                    Currency = ps.Currency,
                                                    Price = ps.Price,
                                                    DefaultImageUrl = string.IsNullOrEmpty(ps.ProductImageDetails
                                                                    .FirstOrDefault(pi => !pi.IsDeleted
                                                                                       && pi.ProductId == ps.Id
                                                                                       && pi.IsDefault == true)
                                                                    .ImageUrl) ? null : string.Concat(mediaBaseUrl, ps.ProductImageDetails
                                                                    .FirstOrDefault(pi => !pi.IsDeleted
                                                                                       && pi.ProductId == ps.Id
                                                                                       && pi.IsDefault == true)
                                                                    .ImageUrl)
                                                }).ToList(),
                                    ProductsCount = x.ProductDetail
                                                    .Where(a => a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
                                                    .Count(),
                                    Currency = x.ProductDetail
                                                .FirstOrDefault(a => a.LocationId == x.Id && !a.IsDeleted)
                                                .Currency,
                                    TotalProductsAmount = !x.ProductDetail.Any() ? null : Convert.ToDouble(x.ProductDetail
                                                           .Where(a => a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
                                                           .Select(b => b.Price)
                                                           .Sum().ToString("#.00")),
                                    WarrantyFrom = x.ProductDetail
                                                    .FirstOrDefault(a => !a.IsDeleted && a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
                                                    .ProductWarrantiesDetails.FirstOrDefault(b => !b.IsDeleted)
                                                    .StartDate,
                                    WarrantyTo = x.ProductDetail
                                                  .FirstOrDefault(a => !a.IsDeleted && a.LocationId == x.Id && dependentProductIds.Contains(a.Id))
                                                  .ProductWarrantiesDetails.FirstOrDefault(b => !b.IsDeleted)
                                                  .EndDate,
                                    UpdatedOn = x.UpdatedOn,
                                    IsMoving = x.IsMoving,
                                    Status = x.Status
                                })
                                .ToListAsync();

                            transferItemResponse.IsProduct = false;
                            transferItemResponse.Locations = locationlst;
                            transferItemResponse.Products = new List<ProductsResponseModel>();
                            transferItemResponseslst.Add(transferItemResponse);
                        }
                    }
                    return new ManagerBaseResponse<List<ItemTransferedHistoryResponse>>()
                    {
                        Result = transferItemResponseslst,
                    };
                }
                return new ManagerBaseResponse<List<ItemTransferedHistoryResponse>>()
                {
                    Message = "No transfered history available!",
                    Result = new List<ItemTransferedHistoryResponse>(),
                    StatusCode = 404
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<List<ItemTransferedHistoryResponse>>()
                {
                    Message = ex.Message,
                    Result = new List<ItemTransferedHistoryResponse>(),
                    StatusCode = 500
                };
            }
        }
        public async Task AddProduct(ProductEntity productModel, long? locationId, string userId)
        {
            try
            {
                if (productModel != null)
                {
                    var productReceipts = await unitOfWork.ReceiptProductRepository
                                    .GetListAsync(x => !x.IsDeleted && x.ProductId == productModel.Id);

                    var productImage = await unitOfWork.ProductImageRepository
                                    .GetListAsync(x => !x.IsDeleted && x.ProductId == productModel.Id);

                    var productWarranties = await unitOfWork.ProductWarrantiesRepository
                                        .GetListAsync(x => !x.IsDeleted && x.ProductId == productModel.Id);

                    var existProductTitleCount = await ProductTitleExistsCount(productModel.Title, userId);
                    var productTitle = productModel.Title;
                    if (existProductTitleCount > 0)
                    {
                        productTitle = string.Concat(productModel.Title, " (" + existProductTitleCount + ")");
                    }

                    var response = await unitOfWork.ProductsRepository
                                .AddAsync(new ProductEntity()
                                {
                                    Name = productModel.Name,
                                    Title = productTitle,
                                    ModelNumber = productModel.ModelNumber,
                                    ManufactureName = productModel.ManufactureName,
                                    VendorName = productModel.VendorName,
                                    SerialNumber = productModel.SerialNumber,
                                    Notes = productModel.Notes,
                                    ProductModelInfoId = productModel.ProductModelInfoId,
                                    LocationId = locationId == null ? productModel.LocationId : (long)locationId,
                                    CategoryId = productModel.CategoryId,
                                    FormData = productModel.FormData,
                                    Currency = productModel.Currency,
                                    Price = productModel.Price,
                                    PurchaseDate = productModel.PurchaseDate,
                                    BarCodeNumber = productModel.BarCodeNumber,
                                    BarCodeData = productModel.BarCodeData,
                                    OtherInfo = productModel.OtherInfo,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow,
                                    IsDeleted = false,
                                    IsMoving = false,
                                    Status = MovedStatus.None
                                });
                    await unitOfWork.CommitAsync();

                    if (response != null && response.Id != 0 && productReceipts.Any())
                    {
                        foreach (var receipt in productReceipts)
                        {
                            var receiptInfo = await unitOfWork.ReceiptRepository
                                   .GetAsync(x => !x.IsDeleted && x.Id == receipt.ReceiptId);

                            if (receiptInfo != null)
                            {
                                var existReceiptNameCount = await ReceiptNameExistsCount(receiptInfo.Name, userId);
                                var receiptName = receiptInfo.Name;
                                if (existReceiptNameCount > 0)
                                {
                                    receiptName = string.Concat(receiptInfo.Name, " (" + existReceiptNameCount + ")");
                                }

                                var receiptResponse = await unitOfWork.ReceiptRepository
                                    .AddAsync(new ReceiptEntity()
                                    {
                                        Name = receiptName,
                                        ImageUrl = receiptInfo.ImageUrl,
                                        PurchaseDate = receiptInfo.PurchaseDate?.ToUniversalTime(),
                                        CreatedBy = userId,
                                        CreatedOn = DateTime.UtcNow,
                                        UpdatedBy = userId,
                                        UpdatedOn = DateTime.UtcNow,
                                        IsDeleted = false
                                    });
                                await unitOfWork.CommitAsync();

                                if (receiptResponse.Id > 0)
                                {
                                    await unitOfWork.ReceiptProductRepository
                                       .AddAsync(new ReceiptProductEntity()
                                       {
                                           ReceiptId = receiptResponse.Id,
                                           ProductId = response.Id,
                                           CreatedBy = userId,
                                           CreatedOn = DateTime.UtcNow,
                                           UpdatedBy = userId,
                                           UpdatedOn = DateTime.UtcNow,
                                           IsDeleted = false
                                       });
                                    await unitOfWork.CommitAsync();
                                }
                            }
                        }
                    }
                    if (response != null && productImage.Count() > 0)
                    {
                        foreach (var image in productImage)
                        {
                            await unitOfWork.ProductImageRepository
                                    .AddAsync(new ProductImageEntity()
                                    {
                                        ProductId = response.Id,
                                        ImageUrl = image.ImageUrl,
                                        FileSize = image.FileSize,
                                        IsDefault = image.IsDefault,
                                        CreatedBy = userId,
                                        CreatedOn = DateTime.UtcNow,
                                        UpdatedBy = userId,
                                        UpdatedOn = DateTime.UtcNow,
                                        IsDeleted = false
                                    });
                            await unitOfWork.CommitAsync();
                        }
                    }
                    if (response != null && productWarranties.Count() > 0)
                    {
                        foreach (var productWarranty in productWarranties)
                        {
                            var existWarrantyNameCount = await WarrantyNameExistsCount(productWarranty.Name, userId);
                            var warrantyName = productWarranty.Name;
                            if (existWarrantyNameCount > 0)
                            {
                                warrantyName = string.Concat(productWarranty.Name, " (" + existWarrantyNameCount + ")");
                            }

                            await unitOfWork.ProductWarrantiesRepository
                                    .AddAsync(new ProductWarrantiesEntity()
                                    {
                                        ProductId = response.Id,
                                        Name = warrantyName,
                                        StartDate = productWarranty.StartDate,
                                        EndDate = productWarranty.EndDate,
                                        Currency = productWarranty.Currency,
                                        Price = productWarranty.Price,
                                        Provider = productWarranty.Provider,
                                        Type = productWarranty.Type,
                                        AgreementNumber = productWarranty.AgreementNumber,
                                        ImageUrl = productWarranty.ImageUrl,
                                        IsProduct = productWarranty.IsProduct,
                                        IsDeleted = false,
                                        CreatedBy = userId,
                                        CreatedOn = DateTime.UtcNow,
                                        UpdatedBy = userId,
                                        UpdatedOn = DateTime.UtcNow,
                                    });
                            await unitOfWork.CommitAsync();
                        }
                    }

                    productModel.Status = MovedStatus.Transferred;
                    productModel.UpdatedOn = DateTime.UtcNow;
                    productModel.UpdatedBy = userId;
                    await unitOfWork.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task DeleteProduct(long productId, string userId)
        {
            try
            {
                var product = await unitOfWork.ProductsRepository
                            .GetAsync(x => !x.IsDeleted && x.Id == productId);

                if (product != null)
                {
                    var productReceipts = await unitOfWork.ReceiptProductRepository
                                        .GetListAsync(x => !x.IsDeleted && x.ProductId == productId);

                    if (productReceipts.Any())
                    {
                        foreach (var receipt in productReceipts)
                        {
                            receipt.IsDeleted = true;
                            receipt.DeletedBy = userId;
                            receipt.DeletedOn = DateTime.UtcNow;
                            await unitOfWork.CommitAsync();
                        }
                    }

                    var productImage = await unitOfWork.ProductImageRepository
                                    .GetListAsync(x => !x.IsDeleted && x.ProductId == productId);

                    if (productImage.Any())
                    {
                        foreach (var image in productImage)
                        {
                            image.IsDeleted = true;
                            image.DeletedBy = userId;
                            image.DeletedOn = DateTime.UtcNow;
                            await unitOfWork.CommitAsync();
                        }
                    }
                    var productWarranties = await unitOfWork.ProductWarrantiesRepository
                                        .GetListAsync(x => !x.IsDeleted && x.ProductId == productId);

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

                    product.IsDeleted = true;
                    product.DeletedBy = userId;
                    product.DeletedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task<int> ProductTitleExistsCount(string productTitle, string userId)
        {
            if (!string.IsNullOrWhiteSpace(productTitle))
            {
                var originalProductTitle = productTitle;
                var count = 1;

                while (await ProductExistsWithTitle(productTitle, userId))
                {
                    productTitle = string.Concat(originalProductTitle, " (", count++, ")");
                }

                return count - 1;
            }

            return 0;
        }
        private async Task<bool> ProductExistsWithTitle(string productTitle, string userId)
        {
            return await unitOfWork.ProductsRepository
                 .AnyAsync(x => !x.IsDeleted
                             && x.CreatedBy == userId && x.Status != MovedStatus.Transferred
                             && x.Title.ToLower() == productTitle.ToLower());
        }
        private async Task<int> LocationNameExistsCount(string locationName, string userId)
        {
            if (!string.IsNullOrWhiteSpace(locationName))
            {
                var originalLocationName = locationName;
                var count = 1;

                while (await LocationExistsWithName(locationName, userId))
                {
                    locationName = string.Concat(originalLocationName, " (", count++, ")");
                }

                return count - 1;
            }

            return 0;
        }
        private async Task<bool> LocationExistsWithName(string locationName, string userId)
        {
            return await unitOfWork.LocationsRepository
                .AnyAsync(x => !x.IsDeleted
                            && x.CreatedBy == userId && x.Status != MovedStatus.Transferred
                            && x.Name.ToLower() == locationName.ToLower());
        }
        private async Task<int> ReceiptNameExistsCount(string receiptName, string userId)
        {
            if (!string.IsNullOrWhiteSpace(receiptName))
            {
                var originalReceiptName = receiptName;
                var count = 1;

                while (await ReceiptExistsWithName(receiptName, userId))
                {
                    receiptName = string.Concat(originalReceiptName, " (", count++, ")");
                }

                return count - 1;
            }

            return 0;
        }
        private async Task<bool> ReceiptExistsWithName(string receiptName, string userId)
        {
            return await unitOfWork.ReceiptRepository
                 .AnyAsync(x => !x.IsDeleted && x.CreatedBy == userId
                             && x.Name.ToLower() == receiptName.ToLower());
        }
        private async Task<int> WarrantyNameExistsCount(string warrantyName, string userId)
        {
            if (!string.IsNullOrWhiteSpace(warrantyName))
            {
                var originalWarrantyName = warrantyName;
                var count = 1;

                while (await WarrantyExistsWithName(warrantyName, userId))
                {
                    warrantyName = string.Concat(originalWarrantyName, " (", count++, ")");
                }

                return count - 1;
            }

            return 0;
        }
        private async Task<bool> WarrantyExistsWithName(string warrantyName, string userId)
        {
            return await unitOfWork.ProductWarrantiesRepository
                 .AnyAsync(x => !x.IsDeleted && x.CreatedBy == userId
                             && x.Name.ToLower() == warrantyName.ToLower());
        }
        private async Task<bool> IsDefaultLocationExists(string userId)
        {
            return await unitOfWork.LocationsRepository
                .AnyAsync(x => !x.IsDeleted
                            && x.CreatedBy == userId
                            && x.Status != MovedStatus.Transferred
                            && x.IsDefault);
        }
        private string GetUserInfo(string transferId)
        {
            var info = unitOfWork.ItemTransferInvitationRepository
                     .GetQueryable(x => !x.IsDeleted && x.TransferId == transferId)
                     .FirstOrDefault();
            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.Email))
                {
                    return info.Email;
                }
                else
                {
                    return string.Concat(info.CountryCode, info.PhoneNumber);
                }
            }
            return string.Empty;
        }
        public async Task<ManagerBaseResponse<bool>> SetItemExpriedStatus()
        {
            try
            {
                DateTime startDateTime = DateTime.Today; //Today at 00:00:00
                DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59

                var itemMovedStatus = new List<MovedStatus>();
                itemMovedStatus.Add(MovedStatus.Initiated);
                itemMovedStatus.Add(MovedStatus.Waiting);

                var result = unitOfWork.ItemTransferRepository
                            .GetQueryable(x => !x.IsDeleted
                                            && itemMovedStatus.Contains(x.Status)
                                            && x.ExpireOn >= startDateTime.ToUniversalTime()
                                            && x.ExpireOn <= endDateTime.ToUniversalTime())
                           .ToList();

                if (result.Any())
                {
                    foreach (var itemTransfer in result)
                    {
                        itemTransfer.Status = MovedStatus.Expired;
                        itemTransfer.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();

                        if (itemTransfer.IsProduct)
                        {
                            var productInfo = await unitOfWork.ProductsRepository
                                            .GetAsync(x => !x.IsDeleted && x.Id == itemTransfer.ItemId);
                            if (productInfo != null)
                            {
                                productInfo.TransferTo = null;
                                productInfo.IsMoving = false;
                                productInfo.Status = MovedStatus.Expired;
                                await unitOfWork.CommitAsync();

                                var title = "The product transfer request expired";
                                var description = "The product transfer request has been expired";
                                var pushModel = new PushNotificationRequestModel()
                                {
                                    RecipientId = itemTransfer.ToUserId,
                                    Title = title,
                                    Description = description,
                                    NotificationType = NotificationType.ItemMove,
                                    ReferenceId = itemTransfer.TransferId,
                                    Status = MovedStatus.Expired
                                };

                                if (!string.IsNullOrEmpty(pushModel.RecipientId))
                                {
                                    await sendNotificationManager.SendItemTransferNotification(pushModel, itemTransfer.FromUserId);
                                }

                                pushModel.RecipientId = itemTransfer.FromUserId;
                                await sendNotificationManager.SendItemTransferNotification(pushModel, itemTransfer.FromUserId);
                            }
                        }
                        else
                        {
                            var locationInfo = await unitOfWork.LocationsRepository
                                            .GetAsync(x => !x.IsDeleted && x.Id == itemTransfer.ItemId);

                            if (locationInfo != null)
                            {
                                locationInfo.TransferTo = null;
                                locationInfo.IsMoving = false;
                                locationInfo.Status = MovedStatus.Expired;
                                await unitOfWork.CommitAsync();

                                var productInfoList = await unitOfWork.ProductsRepository
                                                    .GetQueryable(x => !x.IsDeleted && x.LocationId == locationInfo.Id
                                                                    && itemTransfer.DependentProductIds.Contains(x.Id.ToString()))
                                                    .ToListAsync();

                                if (productInfoList.Any())
                                {
                                    foreach (var productInfo in productInfoList)
                                    {
                                        productInfo.TransferTo = null;
                                        productInfo.IsMoving = false;
                                        productInfo.Status = MovedStatus.Expired;
                                        await unitOfWork.CommitAsync();
                                    }
                                }

                                var title = "The location transfer request expired";
                                var description = " The location transfer request has been expired";
                                var pushModel = new PushNotificationRequestModel()
                                {
                                    RecipientId = itemTransfer.ToUserId,
                                    Title = title,
                                    Description = description,
                                    NotificationType = NotificationType.ItemMove,
                                    ReferenceId = itemTransfer.TransferId,
                                    Status = MovedStatus.Expired
                                };

                                if (!string.IsNullOrEmpty(pushModel.RecipientId))
                                {
                                    await sendNotificationManager.SendItemTransferNotification(pushModel, itemTransfer.FromUserId);
                                }

                                pushModel.RecipientId = itemTransfer.FromUserId;
                                await sendNotificationManager.SendItemTransferNotification(pushModel, itemTransfer.FromUserId);
                            }
                        }
                    }
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Items are set as expired successfully",
                        Result = true,
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "No expired request found",
                    Result = false,
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
