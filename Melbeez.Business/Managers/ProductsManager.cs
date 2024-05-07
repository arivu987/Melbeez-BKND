using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Common;
using Melbeez.Common.Models.Entities;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ProductsManager : IProductsManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        private readonly ISendNotificationManager sendNotificationManager;
        private readonly IMoveProductsToAnotherUserLocationManager moveProductsToAnotherUserLocationManager;
        private readonly UserManager<ApplicationUser> userManager;
        public ProductsManager(IUnitOfWork unitOfWork,
                               IConfiguration configuration,
                               ISendNotificationManager sendNotificationManager,
                               IMoveProductsToAnotherUserLocationManager moveProductsToAnotherUserLocationManager,
                               UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.sendNotificationManager = sendNotificationManager;
            this.moveProductsToAnotherUserLocationManager = moveProductsToAnotherUserLocationManager;
            this.userManager = userManager;
        }
        public async Task<ManagerBaseResponse<IEnumerable<ProductsResponseModel>>> Get(string locationIds, string categoryIds, DateTime? purchaseFromDate, DateTime? purchaseToDate, DateTime? warrantyFromDate, DateTime? warrantyToDate, bool? isTransferItem, PagedListCriteria pagedListCriteria, string userId)
        {
            List<long> intLocationIds = locationIds?.Split(',')
                                       .Select(x => Convert.ToInt64(x))
                                       .ToList();

            List<long> intCategoryIds = categoryIds?.Split(',')
                                       .Select(x => Convert.ToInt64(x))
                                       .ToList();

            var result = await unitOfWork.ProductsRepository
                        .GetQueryable(x => !x.IsDeleted && x.Status != MovedStatus.Transferred && x.CreatedBy == userId)
                        .Include(u => u.ProductCategoriesDetail)
                        .Include(u => u.LocationsDetail)
                        .Include(u => u.ProductImageDetails)
                        .Include(u => u.ProductWarrantiesDetails)
                        .Select(s => new ProductsResponseModel()
                        {
                            ProductId = s.Id,
                            ProductName = s.Name,
                            ProductTitle = s.Title,
                            ModelNumber = s.ModelNumber,
                            ManufacturerName = s.ManufactureName,
                            LocationId = s.LocationId,
                            Location = s.LocationsDetail.Name,
                            CategoryId = s.CategoryId,
                            CategoryName = s.ProductCategoriesDetail.Name,
                            SerialNumber = s.SerialNumber,
                            VendorName = s.VendorName,
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
                            Status = s.Status,
                            Notes = s.Notes,
                            OtherInfo = s.OtherInfo
                        })
                        .WhereIf(!string.IsNullOrWhiteSpace(locationIds), w => intLocationIds.Contains(w.LocationId))
                        .WhereIf(!string.IsNullOrWhiteSpace(categoryIds), w => intCategoryIds.Contains((long)w.CategoryId))
                        .WhereIf(purchaseFromDate.HasValue && !(purchaseToDate.HasValue), w => w.PurchaseDate >= purchaseFromDate.Value.ToUniversalTime())
                        .WhereIf(purchaseToDate.HasValue && !(purchaseFromDate.HasValue), w => w.PurchaseDate <= purchaseToDate.Value.ToUniversalTime())
                        .WhereIf(purchaseFromDate.HasValue && purchaseToDate.HasValue, w => w.PurchaseDate >= purchaseFromDate.Value.ToUniversalTime() && w.PurchaseDate <= purchaseToDate.Value.ToUniversalTime())
                        .WhereIf(warrantyFromDate.HasValue && !(warrantyToDate.HasValue), w => w.WarrantyFrom.Value >= warrantyFromDate.Value.ToUniversalTime())
                        .WhereIf(warrantyToDate.HasValue && !(warrantyFromDate.HasValue), w => w.WarrantyTo.Value <= warrantyToDate.Value.ToUniversalTime())
                        .WhereIf(warrantyFromDate.HasValue && warrantyToDate.HasValue, w => w.WarrantyFrom.Value >= warrantyFromDate.Value.ToUniversalTime() && w.WarrantyTo.Value <= warrantyToDate.Value.ToUniversalTime())
                        .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.ProductName.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                             || x.ProductTitle.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                             || x.SerialNumber.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                             || x.VendorName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                        .AsNoTracking()
                        .ToPagedListAsync(pagedListCriteria, orderByTranslations);

            foreach (var item in result.Data)
            {
                item.DefaultImageUrl = item.DefaultImageUrl == null ? null :  Path.Combine(configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl"), item.DefaultImageUrl);
            }
            return new ManagerBaseResponse<IEnumerable<ProductsResponseModel>>()
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
        public async Task<ManagerBaseResponse<IEnumerable<ProductFormDataRequestModel>>> Get(long id, string userId)
        {
            var imageBasePath = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");

            var result = await unitOfWork
                .ProductsRepository
                .GetQueryable(x => !x.IsDeleted && x.Id == id && x.CreatedBy == userId)
                .Include(u => u.ProductCategoriesDetail)
                .Include(u => u.LocationsDetail)
                .Include(u => u.ProductImageDetails)
                .Include(u => u.ReceiptProductDetails)
                .Select(s => new ProductFormDataRequestModel()
                {
                    ProductId = s.Id,
                    ProductName = s.Name,
                    ProductTitle = s.Title,
                    ModelNumber = s.ModelNumber,
                    ManufacturerName = s.ManufactureName,
                    LocationId = s.LocationId,
                    Location = s.LocationsDetail.Name,
                    CategoryId = s.CategoryId,
                    CategoryName = s.ProductCategoriesDetail.Name,
                    SerialNumber = s.SerialNumber,
                    VendorName = s.VendorName,
                    PurchaseDate = s.PurchaseDate,
                    Currency = s.Currency,
                    Price = Convert.ToDouble(s.Price.ToString("#.00")),
                    ProductFormData = s.FormData,
                    ProductImages = s.ProductImageDetails
                                     .Where(p => !p.IsDeleted && p.ProductId == s.Id)
                                     .Select(pi => new ProductImageResponseModel()
                                     {
                                         Id = pi.Id,
                                         ImageUrl = pi.ImageUrl == null ? pi.ImageUrl : Path.Combine(imageBasePath, pi.ImageUrl),
                                         FileSize = pi.FileSize,
                                         IsDefault = pi.IsDefault
                                     }).ToList(),
                    BarCodeNumber = s.BarCodeNumber,
                    ReceiptId = s.ReceiptProductDetails
                                 .FirstOrDefault(x => !x.IsDeleted && x.ProductId == s.Id)
                                 .ReceiptId,
                    HasWarranty = s.ProductWarrantiesDetails.Where(x => !x.IsDeleted).Count() > 0 ? true : false,
                    IsMoving = s.IsMoving,
                    Status = s.Status,
                    Notes = s.Notes,
                    OtherInfo = s.OtherInfo
                })
                .AsNoTracking()
                .ToListAsync();

            foreach (var item in result)
            {
                string toUserName = null;
                var productTransferDetails = await unitOfWork.ItemTransferRepository
                    .GetAsync(x => !x.IsDeleted && x.Status != MovedStatus.Rejected
                                && x.Status != MovedStatus.Cancelled 
                                && !x.IsProduct ? x.ItemId == item.LocationId && x.DependentProductIds.Contains(id.ToString()) : x.ItemId == id);

                if (productTransferDetails != null)
                {
                    var toUserData = userManager.Users.FirstOrDefault(x => !x.IsDeleted && x.Id == productTransferDetails.ToUserId);
                    toUserName = toUserData == null
                    ? GetUserInfo(productTransferDetails.TransferId)
                    : toUserData.FirstName + ' ' + toUserData.LastName;
                }
                item.TransferId = productTransferDetails == null ? null : productTransferDetails.TransferId;
                item.TransferTo = toUserName;
                item.TransferExpireOn = productTransferDetails == null ? null : productTransferDetails.ExpireOn;
                item.TransferInitiatedOn = productTransferDetails == null ? null : productTransferDetails.CreatedOn;
            }

            return new ManagerBaseResponse<IEnumerable<ProductFormDataRequestModel>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> DeleteProduct(List<long> Ids, string userId)
        {
            try
            {
                foreach (var id in Ids)
                {
                    var entity = await unitOfWork
                                .ProductsRepository
                                .GetAsync(entity => entity.Id == id && !entity.IsDeleted);

                    if (entity != null)
                    {
                        #region Get Related Warranties and update IsDeleted true.

                        var warranties = await unitOfWork
                                        .ProductWarrantiesRepository
                                        .GetListAsync(w => !w.IsDeleted && w.ProductId == entity.Id && w.CreatedBy == userId);

                        if (warranties.Any())
                        {
                            foreach (var warranty in warranties)
                            {
                                warranty.IsDeleted = true;
                                warranty.DeletedBy = userId;
                                warranty.DeletedOn = DateTime.UtcNow;
                                await unitOfWork.CommitAsync();
                            }
                        }

                        #endregion

                        #region Get Related Images and update IsDeleted true

                        var productImages = await unitOfWork
                                           .ProductImageRepository
                                           .GetListAsync(i => !i.IsDeleted && i.ProductId == entity.Id && i.CreatedBy == userId);

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

                        #region Get Related Receiptes and update IsDeleted true

                        var receiptsList = await unitOfWork
                                          .ReceiptProductRepository
                                          .GetListAsync(w => !w.IsDeleted && w.ProductId == entity.Id && w.CreatedBy == userId);

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

                        entity.IsDeleted = true;
                        entity.DeletedBy = userId;
                        entity.DeletedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }

                    await sendNotificationManager.SendProductUpdateNotification(new PushNotificationRequestModel()
                    {
                        RecipientId = userId,
                        Title = "Product has been deleted",
                        Description = "The product " + entity.Name + " has been deleted",
                        NotificationType = NotificationType.ProductUpdate,
                        ReferenceId = entity.Id.ToString()
                    }, userId);
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product(s) deleted successfully.",
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
        public async Task<ManagerBaseResponse<bool>> MoveProductsLocation(MoveProductsLocationRequestModel model, string userId)
        {
            try
            {
                foreach (var id in model.ProductsId)
                {
                    var locationName = await unitOfWork.LocationsRepository.GetAsync(x => x.Id == model.LocationId);
                    var entity = await unitOfWork.ProductsRepository.GetAsync(entity => entity.Id == id
                                        && entity.CreatedBy == userId && !entity.IsDeleted);
                    if (entity != null)
                    {
                        entity.LocationId = model.LocationId;
                        entity.UpdatedBy = userId;
                        entity.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }

                    await sendNotificationManager.SendProductUpdateNotification(new PushNotificationRequestModel()
                    {
                        RecipientId = userId,
                        Title = "Product has been moved",
                        Description = "Product " + entity.Name + " have been moved to " + locationName.Name + " location",
                        NotificationType = NotificationType.ProductUpdate,
                        ReferenceId = entity.Id.ToString()
                    }, userId);
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product(s) moved successfully.",
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
        public async Task<ManagerBaseResponse<long?>> AddProduct(ProductRequestModel model, string userId)
        {
            try
            {
                var existsProductModelInfo = await unitOfWork.ProductModelInformationRepository
                    .GetAsync(x => !x.IsDeleted && x.ManufacturerName.ToLower() == model.ManufacturerName.ToLower()
                                && x.ModelNumber.ToLower() == model.ModelNumber.ToLower());

                if (existsProductModelInfo != null)
                {
                    model.ProductModelInfoId = existsProductModelInfo.Id;
                }

                var existProductTitleCount = await productTitleExistsCount(model.ProductTitle, userId);
                var productTitle = model.ProductTitle;
                if (existProductTitleCount > 0)
                {
                    productTitle = string.Concat(model.ProductTitle, " (" + existProductTitleCount + ")");
                }

                var response = await unitOfWork.ProductsRepository
                        .AddAsync(new ProductEntity()
                        {
                            Name = model.ProductName,
                            Title = productTitle,
                            LocationId = model.LocationId,
                            ModelNumber = model.ModelNumber,
                            ManufactureName = model.ManufacturerName,
                            SerialNumber = model.SerialNumber,
                            VendorName = model.VendorName,
                            Notes = model.Notes,
                            CategoryId = model.CategoryId,
                            FormData = existsProductModelInfo != null ? existsProductModelInfo.FormBuilderData : null,
                            Currency = model.Currency,
                            Price = model.Price,
                            PurchaseDate = model.PurchaseDate.HasValue ? model.PurchaseDate.Value.ToUniversalTime() : null,
                            BarCodeNumber = model.BarCodeNumber,
                            BarCodeData = model.BarCodeData,
                            OtherInfo = model.OtherInfo,
                            CreatedBy = userId,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedBy = userId,
                            UpdatedOn = DateTime.UtcNow,
                            IsDeleted = false,
                            IsMoving = false,
                            Status = MovedStatus.None,
                            ProductModelInfoId = model.ProductModelInfoId
                        });
                await unitOfWork.CommitAsync();

                if (response != null && response.Id != 0)
                {
                    /** Added Product Receipts **/
                    if (model.ReceiptId != 0)
                    {
                        await unitOfWork.ReceiptProductRepository
                            .AddAsync(new ReceiptProductEntity()
                            {
                                ReceiptId = model.ReceiptId,
                                ProductId = response.Id,
                                CreatedBy = userId,
                                CreatedOn = DateTime.UtcNow,
                                UpdatedBy = userId,
                                UpdatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        await unitOfWork.CommitAsync();
                    }

                    /** Added Product Images **/
                    if (model.Files.Any())
                    {
                        foreach (var item in model.Files)
                        {
                            await unitOfWork.ProductImageRepository
                                .AddAsync(new ProductImageEntity()
                                {
                                    ProductId = response.Id,
                                    ImageUrl = item.FileUrl,
                                    FileSize = item.FileSize,
                                    IsDefault = item.IsDefault,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow,
                                    IsDeleted = false
                                });
                            await unitOfWork.CommitAsync();
                        }
                    }

                    /** Added Product Warrenty Cards **/
                    if (model.productWarrantiesRequestModels.Any())
                    {
                        foreach (var item in model.productWarrantiesRequestModels)
                        {
                            await unitOfWork.ProductWarrantiesRepository
                                .AddAsync(new ProductWarrantiesEntity()
                                {
                                    ProductId = response.Id,
                                    Name = item.Name,
                                    StartDate = item.StartDate.ToUniversalTime(),
                                    EndDate = item.EndDate.ToUniversalTime(),
                                    Currency = item.Currency,
                                    Price = item.Price,
                                    Provider = item.Provider,
                                    Type = item.Type,
                                    AgreementNumber = item.AgreementNumber,
                                    ImageUrl = item.ImageUrl,
                                    IsProduct = item.IsProduct,
                                    IsDeleted = false,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow,
                                });
                            await unitOfWork.CommitAsync();
                        }
                    }
                }

                /** Added Product Models **/
                if (existsProductModelInfo == null)
                {
                    var productmodel = await unitOfWork.ProductModelInformationRepository
                        .AddAsync(new ProductModelInformationEntity()
                        {
                            ModelNumber = model.ModelNumber,
                            ManufacturerName = model.ManufacturerName,
                            Status = ProductModelStatus.Pendding,
                            CreatedBy = userId,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedBy = userId,
                            UpdatedOn = DateTime.UtcNow,
                            IsDeleted = false,
                        });
                    await unitOfWork.CommitAsync();

                    var entity = await unitOfWork.ProductsRepository
                            .GetAsync(x => !x.IsDeleted && x.Id == response.Id
                                        && x.CreatedBy == userId);
                    entity.ProductModelInfoId = productmodel.Id;
                    await unitOfWork.CommitAsync();
                }

                await sendNotificationManager.SendProductUpdateNotification(new PushNotificationRequestModel()
                {
                    RecipientId = userId,
                    Title = "New product has been added.",
                    Description = "New product " + model.ProductTitle + " with model number " + model.ModelNumber + " and manufacturer name " + model.ManufacturerName + " has been added.",
                    NotificationType = NotificationType.ProductUpdate,
                    ReferenceId = response.Id.ToString()
                }, userId);

                return new ManagerBaseResponse<long?>()
                {
                    Message = "Product added successfully.",
                    Result = response.Id
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<long?>()
                {
                    Message = ex.Message,
                    Result = null
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateProduct(long productId, ProductRequestModel model, string userId)
        {
            try
            {
                var existsProductModelInfo = await unitOfWork.ProductModelInformationRepository
                    .GetAsync(x => !x.IsDeleted && x.ManufacturerName.ToLower() == model.ManufacturerName.ToLower()
                                && x.ModelNumber.ToLower() == model.ModelNumber.ToLower());

                if (existsProductModelInfo != null)
                {
                    model.ProductModelInfoId = existsProductModelInfo.Id;
                }

                var formFields = model.FormFields;
                var entity = await unitOfWork
                            .ProductsRepository
                            .GetAsync(entity => entity.Id == productId && entity.CreatedBy == userId && !entity.IsDeleted);

                if (entity != null)
                {
                    entity.Name = model.ProductName ?? entity.Name;
                    entity.Title = model.ProductTitle ?? entity.Title;
                    entity.LocationId = model.LocationId;
                    entity.ModelNumber = model.ModelNumber;
                    entity.ManufactureName = model.ManufacturerName;
                    entity.SerialNumber = model.SerialNumber;
                    entity.VendorName = model.VendorName;
                    entity.Notes = model.Notes;
                    entity.CategoryId = model.CategoryId;
                    entity.FormData = existsProductModelInfo != null ? existsProductModelInfo.FormBuilderData : entity.FormData;
                    entity.Currency = model.Currency ?? entity.Currency;
                    entity.Price = model.Price;
                    entity.PurchaseDate = model.PurchaseDate.HasValue ? model.PurchaseDate.Value.ToUniversalTime() : null;
                    entity.OtherInfo = model.OtherInfo ?? entity.OtherInfo; ;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }

                if (productId != 0 && model.ReceiptId != 0)
                {
                    var entityReceipt = await unitOfWork
                                .ReceiptProductRepository
                                .GetAsync(entityReceipt => entityReceipt.ProductId == productId
                                                        && entityReceipt.CreatedBy == userId
                                                        && !entityReceipt.IsDeleted);

                    if (entityReceipt != null)
                    {
                        entityReceipt.ReceiptId = model.ReceiptId;
                        entityReceipt.ProductId = productId;
                        entityReceipt.UpdatedOn = DateTime.UtcNow;
                        entityReceipt.UpdatedBy = userId;
                        await unitOfWork.CommitAsync();
                    }
                    else
                    {
                        await unitOfWork
                            .ReceiptProductRepository
                            .AddAsync(new ReceiptProductEntity()
                            {
                                ReceiptId = model.ReceiptId,
                                ProductId = productId,
                                CreatedBy = userId,
                                CreatedOn = DateTime.UtcNow,
                                UpdatedBy = userId,
                                UpdatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            });
                        await unitOfWork.CommitAsync();
                    }
                }
                if (productId != 0 && model.Files.Count() > 0)
                {
                    foreach (var item in model.Files)
                    {
                        if (item.IsDefault)
                        {
                            var entityImage = await unitOfWork
                               .ProductImageRepository
                               .GetAsync(entityImage => entityImage.IsDefault
                                           && entityImage.ProductId == productId && entityImage.CreatedBy == userId
                                           && !entityImage.IsDeleted);
                            if (entityImage != null)
                            {
                                entityImage.IsDefault = false;
                                entityImage.UpdatedOn = DateTime.UtcNow;
                                entityImage.UpdatedBy = userId;
                                await unitOfWork.CommitAsync();
                            }
                        }

                        await unitOfWork
                            .ProductImageRepository
                            .AddAsync(new ProductImageEntity()
                            {
                                ProductId = productId,
                                ImageUrl = item.FileUrl,
                                FileSize = item.FileSize,
                                IsDefault = item.IsDefault,
                                CreatedBy = userId,
                                CreatedOn = DateTime.UtcNow,
                                UpdatedBy = userId,
                                UpdatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            });

                        await unitOfWork.CommitAsync();
                    }
                }

                await sendNotificationManager.SendProductUpdateNotification(new PushNotificationRequestModel()
                {
                    RecipientId = userId,
                    Title = "Product has been updated",
                    Description = "Product details have been updated for the model number " + model.ModelNumber,
                    NotificationType = NotificationType.ProductUpdate,
                    ReferenceId = productId.ToString()
                }, userId);

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product updated successfully.",
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
        string GetFieldValue(List<FormFieldRequestModel> formFields, string fieldName)
        {
            return formFields.First(x => x.Field == fieldName)?.Value;
        }
        public async Task<ManagerBaseResponse<bool>> AddProductImages(long productId, List<FileBaseRequest> model, string userId)
        {
            try
            {
                foreach (var item in model)
                {
                    await unitOfWork
                        .ProductImageRepository
                        .AddAsync(new ProductImageEntity()
                        {
                            ProductId = productId,
                            ImageUrl = item.FileUrl,
                            FileSize = item.FileSize,
                            IsDefault = item.IsDefault,
                            CreatedBy = userId,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedBy = userId,
                            UpdatedOn = DateTime.UtcNow,
                            IsDeleted = false
                        });
                    await unitOfWork.CommitAsync();
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product Image(s) added successfully.",
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
        public async Task<ManagerBaseResponse<bool>> SetDefaultProductImage(long productId, long imageId, string userId)
        {
            try
            {
                var entity = await unitOfWork
                                .ProductImageRepository
                                .GetQueryable(entity => entity.ProductId == productId
                                            && entity.CreatedBy == userId
                                            && !entity.IsDeleted).ToListAsync();
                foreach (var item in entity)
                {
                    if (item.Id != imageId && item.IsDefault)
                    {
                        item.IsDefault = false;
                        item.UpdatedBy = userId;
                        item.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();

                    }
                    if (item.Id == imageId && !item.IsDefault)
                    {
                        item.IsDefault = true;
                        item.UpdatedBy = userId;
                        item.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Set as default image successfully.",
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
        public async Task<ManagerBaseResponse<bool>> DeleteProductImages(List<long> imageids, long productId, string userId)
        {
            try
            {
                foreach (var id in imageids)
                {
                    var entity = await unitOfWork
                                    .ProductImageRepository
                                    .GetAsync(entity => entity.Id == id
                                                && entity.ProductId == productId
                                                && entity.CreatedBy == userId
                                                && !entity.IsDeleted);
                    if (entity != null)
                    {
                        entity.IsDeleted = true;
                        entity.DeletedBy = userId;
                        entity.DeletedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Image(s) deleted successfully.",
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
        private async Task<bool> IsProductNameAlreadyExists(long? id, string productName, string userId)
        {
            bool isAlreadyExists = await unitOfWork
                             .ProductsRepository
                             .GetQueryable(x => !x.IsDeleted && x.CreatedBy == userId
                                             && x.Name.ToLower().Equals(productName.ToLower()))
                             .WhereIf(id != null, x => x.Id != id)
                             .AnyAsync();
            return isAlreadyExists;
        }
        public async Task<ManagerBaseResponse<bool>> MoveProductsToAnotherUserLocation(MoveProductsToAnotherUserLocationRequestModel model, string fromUserId)
        {
            try
            {
                foreach (var productId in model.ProductIds)
                {

                    var result = await moveProductsToAnotherUserLocationManager
                                 .AddTransferedItem(new AddMoveProductsRequestModel()
                                 {
                                     ProductId = productId,
                                     LocationId = model.LocationId,
                                     ToUserId = model.ToUserId
                                 }, fromUserId);

                    var productData = await unitOfWork
                                .ProductsRepository
                                .GetAsync(x => !x.IsDeleted
                                            && x.Id == productId
                                            && x.CreatedBy == fromUserId);
                    productData.IsMoving = true;
                    await unitOfWork.CommitAsync();

                    var userData = userManager
                                  .Users
                                  .Where(x => !x.IsDeleted
                                           && x.Id == fromUserId)
                                  .FirstOrDefault();
                    var senderName = string.Concat(userData.FirstName, " ", userData.LastName);
                    await sendNotificationManager.SendItemTransferNotification(new PushNotificationRequestModel()
                    {
                        RecipientId = model.ToUserId,
                        Title = "Product move request from " + senderName,
                        Description = senderName + " wants to move '" + productData.Title + "' product to you.",
                        NotificationType = NotificationType.ItemMove,
                        ReferenceId = result.Result.ToString(),
                        Status = MovedStatus.Waiting,
                    }, fromUserId);
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product(s) move request sent successfully.",
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
        public async Task<ManagerBaseResponse<bool>> CancelProductMoveRequest(long productId, MovedStatus status, string userId)
        {
            try
            {
                var result = await unitOfWork
                            .MoveProductsToAnotherUserLocationRepository
                            .GetAsync(x => !x.IsDeleted
                                        //&& x.ProductId == productId
                                        && x.CreatedBy == userId
                                        && x.Status == MovedStatus.Waiting);
                if (result != null)
                {
                    result.Status = status;
                    result.UpdatedOn = DateTime.Now;
                    result.UpdatedBy = userId;
                    await unitOfWork.CommitAsync();

                    var productData = await unitOfWork
                                    .ProductsRepository
                                    .GetAsync(x => !x.IsDeleted
                                                && x.Id == productId
                                                && x.CreatedBy == userId);
                    productData.IsMoving = false;
                    productData.UpdatedOn = DateTime.Now;
                    productData.UpdatedBy = userId;
                    await unitOfWork.CommitAsync();

                    var pushNotificationData = await unitOfWork
                                               .PushNotificationRepositry
                                               .GetAsync(x => !x.IsDeleted
                                                            && x.IsSuccess
                                                            && x.Type == NotificationType.ItemMove
                                                            && x.ReferenceId == result.Id.ToString());
                    if (pushNotificationData != null)
                    {
                        pushNotificationData.Status = status;
                        pushNotificationData.UpdatedOn = DateTime.Now;
                        pushNotificationData.UpdatedBy = userId;
                        await unitOfWork.CommitAsync();
                    }

                    var userData = userManager
                                      .Users
                                      .Where(x => !x.IsDeleted
                                               && x.Id == userId)
                                      .FirstOrDefault();
                    var senderName = string.Concat(userData.FirstName, " ", userData.LastName);
                    await sendNotificationManager.SendItemTransferNotification(new PushNotificationRequestModel()
                    {
                        RecipientId = result.ToUserId,
                        Title = "Product move request cancelled",
                        Description = senderName + " cancelled move request for '" + productData.Name + "' product to you.",
                        NotificationType = NotificationType.ItemMove,
                        ReferenceId = null,
                        Status = MovedStatus.Cancelled
                    }, userId);
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product(s) move request cancelled successfully.",
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
        private async Task<int> productTitleExistsCount(string productTitle, string userId)
        {
            if (!string.IsNullOrWhiteSpace(productTitle))
            {
                var originalProductTitle = productTitle;
                var count = 1;

                while (await productExistsWithTitle(productTitle, userId))
                {
                    productTitle = string.Concat(originalProductTitle, " (", count++, ")");
                }

                return count - 1;
            }

            return 0;
        }
        private async Task<bool> productExistsWithTitle(string productTitle, string userId)
        {
            return await unitOfWork.ProductsRepository
                 .AnyAsync(x => !x.IsDeleted
                             && x.CreatedBy == userId && x.Status != MovedStatus.Transferred
                             && x.Title.ToLower() == productTitle.ToLower());
        }
    }
}
