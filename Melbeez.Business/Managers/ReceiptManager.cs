using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ReceiptManager : IReceiptManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public ReceiptManager(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
        }
        public async Task<ManagerBaseResponse<IEnumerable<ReceiptResponseModel>>> Get(DateTime? purchaseFromDate, DateTime? purchaseToDate, PagedListCriteria pagedListCriteria, string userId)
        {
            var baseImagePath = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var result = await unitOfWork.ReceiptRepository
                .GetQueryable(x => !x.IsDeleted && x.CreatedBy == userId)
                .Include(u => u.ReceiptProductDetails)
                .Select(x => new ReceiptResponseModel()
                {
                    ReceiptId = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl == null ? x.ImageUrl : Path.Combine(baseImagePath, x.ImageUrl),
                    PurchaseDate = x.PurchaseDate,
                    ProductAsscociatedCount = x.ReceiptProductDetails.Where(r => !r.IsDeleted && r.productDetail.Status != MovedStatus.Transferred && r.ReceiptId == x.Id).Count(),
                    CreatedOn = x.CreatedOn
                })
                .WhereIf(purchaseFromDate.HasValue && !(purchaseToDate.HasValue), w => w.PurchaseDate.Value >= purchaseFromDate.Value.ToUniversalTime())
                .WhereIf(purchaseToDate.HasValue && !(purchaseFromDate.HasValue), w => w.PurchaseDate.Value <= purchaseToDate.Value.ToUniversalTime())
                .WhereIf(purchaseFromDate.HasValue && purchaseToDate.HasValue, w => w.PurchaseDate.Value >= purchaseFromDate.Value.ToUniversalTime() && w.PurchaseDate.Value <= purchaseToDate.Value.ToUniversalTime())
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.Name.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<ReceiptResponseModel>>()
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
        public async Task<ManagerBaseResponse<IEnumerable<ReceiptDetailResponseModel>>> Get(long id, string userId)
        {
            var baseImagePath = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var result = await unitOfWork.ReceiptRepository
                        .GetQueryable(x => !x.IsDeleted && x.Id == id && x.CreatedBy == userId)
                        .Include(u => u.ReceiptProductDetails)
                        .Select(x => new ReceiptDetailResponseModel()
                        {
                            ReceiptId = x.Id,
                            Name = x.Name,
                            ImageUrl = x.ImageUrl == null ? x.ImageUrl : Path.Combine(baseImagePath, x.ImageUrl),
                            PurchaseDate = x.PurchaseDate,
                            ProductAsscociatedCount = x.ReceiptProductDetails.Where(r => !r.IsDeleted && r.productDetail.Status != MovedStatus.Transferred && r.ReceiptId == x.Id).Count(),
                            ProductDetails = x.ReceiptProductDetails.Where(s => !s.IsDeleted && s.productDetail.Status != MovedStatus.Transferred && x.Id == id)
                                             .Select(p => new ProductsResponseModel()
                                             {
                                                 ProductId = p.productDetail.Id,
                                                 ProductName = p.productDetail.Name,
                                                 LocationId = p.productDetail.LocationId,
                                                 Location = p.productDetail.LocationsDetail.Name,
                                                 CategoryId = p.productDetail.CategoryId,
                                                 CategoryName = p.productDetail.ProductCategoriesDetail.Name,
                                                 PurchaseDate = p.productDetail.PurchaseDate,
                                                 Currency = p.productDetail.Currency,
                                                 Price = p.productDetail.Price,
                                                 DefaultImageUrl = p.productDetail.ProductImageDetails.First(a => !a.IsDeleted && a.ProductId == p.productDetail.Id && a.IsDefault == true).ImageUrl == null
                                                                   ? null : Path.Combine(baseImagePath, p.productDetail.ProductImageDetails.First(a => !a.IsDeleted && a.ProductId == p.productDetail.Id && a.IsDefault == true).ImageUrl),
                                             }).ToList()
                        }).ToListAsync();

            return new ManagerBaseResponse<IEnumerable<ReceiptDetailResponseModel>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddReceipt(ReceiptRequestModel model, string userId)
        {
            try
            {
                var isExists = await IsReceiptAlreadyExists(null, model.ReceiptName, userId);
                if (isExists)
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Receipt name already exists",
                        Result = false,
                        StatusCode = 500
                    };
                }

                await unitOfWork.ReceiptRepository
                    .AddAsync(new ReceiptEntity()
                    {
                        Name = model.ReceiptName,
                        ImageUrl = model.ReceiptImageUrl,
                        PurchaseDate = model.PurchaseDate?.ToUniversalTime(),
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = userId,
                        UpdatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    });

                await unitOfWork.CommitAsync();
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Receipt added successfully.",
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
        public async Task<ManagerBaseResponse<bool>> UpdateReceipt(ReceiptRequestModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                           .ReceiptRepository
                           .GetAsync(entity => entity.Id == model.ReceiptId
                                       && entity.CreatedBy == userId
                                       && !entity.IsDeleted);
                if (entity != null)
                {
                    var isExists = await IsReceiptAlreadyExists(model.ReceiptId, model.ReceiptName, userId);
                    if (isExists)
                    {
                        return new ManagerBaseResponse<bool>()
                        {
                            Message = "Receipt name already exists",
                            Result = false,
                            StatusCode = 500
                        };
                    }

                    entity.Name = model.ReceiptName ?? entity.Name;
                    entity.ImageUrl = model.ReceiptImageUrl ?? entity.ImageUrl;
                    entity.PurchaseDate = model.PurchaseDate?.ToUniversalTime() ?? entity.PurchaseDate;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Receipt updated successfully.",
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
        public async Task<ManagerBaseResponse<bool>> DeleteReceipt(long id, string userId)
        {
            try
            {
                var entity = await unitOfWork
                                .ReceiptRepository
                                .GetAsync(entity => entity.Id == id && entity.CreatedBy == userId
                                            && !entity.IsDeleted);
                if (entity != null)
                {
                    #region Delete Receipts from associated products

                    var receiptsList = await unitOfWork
                                      .ReceiptProductRepository
                                      .GetListAsync(w => !w.IsDeleted && w.ReceiptId == entity.Id && w.CreatedBy == userId);

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
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Receipt deleted successfully.",
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
        private async Task<bool> IsReceiptAlreadyExists(long? id, string receiptName, string userId)
        {
            bool isAlreadyExists = await unitOfWork
                              .ReceiptRepository
                              .GetQueryable(x => !x.IsDeleted && x.CreatedBy == userId
                                              && x.Name.ToLower().Equals(receiptName.ToLower()))
                              .WhereIf(id != null, x => x.Id != id)
                              .AnyAsync();
            return isAlreadyExists;
        }
    }
}