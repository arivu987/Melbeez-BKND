using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
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
    public class ProductWarrantiesManager : IProductWarrantiesManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        private readonly ISendNotificationManager sendNotificationManager;
        public ProductWarrantiesManager(IUnitOfWork unitOfWork, IConfiguration configuration, ISendNotificationManager sendNotificationManager)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.sendNotificationManager = sendNotificationManager;
        }
        public async Task<ManagerBaseResponse<IEnumerable<BaseWarrantiesResponseModel>>> Get(WarrantyStatus? status, string categoryIds, DateTime? warrantyFromDate, DateTime? warrantyToDate, PagedListCriteria pagedListCriteria, string userId)
        {
            var baseImageUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            List<long> intCategoryIds = categoryIds?
                                       .Split(',')
                                       .Select(x => Convert.ToInt64(x))
                                       .ToList();
            var result = await unitOfWork
                .ProductWarrantiesRepository
                .GetQueryable(x => !x.IsDeleted && x.IsProduct == true && x.CreatedBy == userId)
                .Include(x => x.ProductDetail)
                .Select(x => new BaseWarrantiesResponseModel()
                {
                    ProductWarrantyId = x.Id,
                    Name = x.Name,
                    CategoryId = x.ProductDetail.CategoryId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductDetail.Name,
                    PurchaseDate = x.ProductDetail.PurchaseDate,
                    ImageUrl = x.ImageUrl == null ? x.ImageUrl : Path.Combine(baseImageUrl, x.ImageUrl),
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = GetStatus(x.EndDate),
                    ProductStatus = x.ProductDetail.Status,
                    CreatedOn = x.CreatedOn
                })
                .WhereIf(!string.IsNullOrWhiteSpace(status.ToString()) && status.ToString() == "Expired", w => w.EndDate.Date < DateTime.UtcNow.Date)
                .WhereIf(!string.IsNullOrWhiteSpace(status.ToString()) && status.ToString() == "Active", w => w.EndDate.Date > DateTime.UtcNow.Date)
                .WhereIf(!string.IsNullOrWhiteSpace(categoryIds), w => intCategoryIds.Contains((long)w.CategoryId))
                .WhereIf(warrantyFromDate.HasValue && !(warrantyToDate.HasValue), w => w.StartDate >= warrantyFromDate.Value.ToUniversalTime())
                .WhereIf(warrantyToDate.HasValue && !(warrantyFromDate.HasValue), w => w.EndDate <= warrantyToDate.Value.ToUniversalTime())
                .WhereIf(warrantyFromDate.HasValue && warrantyToDate.HasValue, w => w.StartDate >= warrantyFromDate.Value.ToUniversalTime() && w.EndDate <= warrantyToDate.Value.ToUniversalTime())
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.Name.ToLower().Contains(pagedListCriteria.SearchText.ToLower()) 
                                                                                    || x.ProductName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);

            return new ManagerBaseResponse<IEnumerable<BaseWarrantiesResponseModel>>()
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
        public async Task<ManagerBaseResponse<IEnumerable<ProductWarrantiesResponseModel>>> Get(long productId, long? id, string userId)
        {
            var baseImageUrl = configuration.GetValue<string>("MediaUploadConfiguration:MediaBaseUrl");
            var result = await unitOfWork
                .ProductWarrantiesRepository
                .GetQueryable(x => !x.IsDeleted && x.ProductId == productId && x.CreatedBy == userId)
                .Include(u => u.ProductDetail)
                .WhereIf(id != null, x => x.Id == id)
                .Select(s => new ProductWarrantiesResponseModel()
                {
                    ProductWarrantyId = s.Id,
                    Name = s.Name,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    Currency = s.Currency,
                    Price = s.Price,
                    Provider = s.Provider,
                    Type = s.Type,
                    AgreementNumber = s.AgreementNumber,
                    ImageUrl = s.ImageUrl == null ? s.ImageUrl : Path.Combine(baseImageUrl, s.ImageUrl),
                    ProductId = s.ProductId,
                    ProductName = s.ProductDetail.Name,
                    PurchaseDate = s.ProductDetail.PurchaseDate,
                    ProductStatus = s.ProductDetail.Status,
                    IsProduct = s.IsProduct,
                    CreatedOn = s.CreatedOn
                })
                .ToListAsync();

            return new ManagerBaseResponse<IEnumerable<ProductWarrantiesResponseModel>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddWarranty(List<ProductWarrantiesRequestModel> model, string userId)
        {
            try
            {
                foreach (var item in model)
                {
                    if (item.Id == 0)
                    {
                        var existWarrantyNameCount = await WarrantyNameExistsCount(null, item.Name, userId);
                        var warrantyName = item.Name;
                        if (existWarrantyNameCount > 0)
                        {
                            warrantyName = string.Concat(item.Name, " (" + existWarrantyNameCount + ")");
                        }

                        var response = await unitOfWork
                            .ProductWarrantiesRepository
                            .AddAsync(new ProductWarrantiesEntity()
                            {
                                ProductId = item.ProductId,
                                Name = warrantyName,
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
                var productId = model.Select(x => x.ProductId).FirstOrDefault();
                var productName = await unitOfWork.ProductsRepository.GetAsync(x => x.Id == productId);
                await sendNotificationManager.SendLocationUpdateNotification(new PushNotificationRequestModel()
                {
                    RecipientId = userId,
                    Title = "New warranty information has been added.",
                    Description = "New warranty information for " + productName.Name + " has been added",
                    NotificationType = NotificationType.ProductUpdate,
                    ReferenceId = null
                }, userId);
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Warranty added successfully.",
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
        public async Task<ManagerBaseResponse<bool>> UpdateWarranty(List<ProductWarrantiesRequestModel> model, string userId)
        {
            try
            {
                foreach (var item in model)
                {

                    var entity = await unitOfWork.ProductWarrantiesRepository
                            .GetAsync(entity => !entity.IsDeleted && entity.Id == item.Id
                                        && entity.ProductId == item.ProductId && entity.CreatedBy == userId);
                    if (entity != null)
                    {
                        var existWarrantyNameCount = await WarrantyNameExistsCount(item.Id, item.Name, userId);
                        var warrantyName = item.Name;
                        if (existWarrantyNameCount > 0)
                        {
                            warrantyName = string.Concat(item.Name, " (" + existWarrantyNameCount + ")");
                        }

                        entity.ProductId = item.ProductId;
                        entity.Name = warrantyName;
                        entity.StartDate = item.StartDate.ToUniversalTime();
                        entity.EndDate = item.EndDate.ToUniversalTime();
                        entity.Currency = item.Currency;
                        entity.Price = item.Price;
                        entity.Provider = item.Provider;
                        entity.Type = item.Type;
                        entity.AgreementNumber = item.AgreementNumber;
                        entity.ImageUrl = item.ImageUrl;
                        entity.IsProduct = item.IsProduct;
                        entity.UpdatedBy = userId;
                        entity.UpdatedOn = DateTime.UtcNow;
                        await unitOfWork.CommitAsync();
                    }
                }

                var productId = model.Select(x => x.ProductId).FirstOrDefault();
                var productData = await unitOfWork.ProductsRepository.GetAsync(x => x.Id == productId);
                await sendNotificationManager.SendLocationUpdateNotification(new PushNotificationRequestModel()
                {
                    RecipientId = userId,
                    Title = "Warranty information has been updated",
                    Description = "Warranty information has been updated for " + productData.Name,
                    NotificationType = NotificationType.ProductUpdate,
                    ReferenceId = null
                }, userId);

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Warranty updated successfully.",
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
        public async Task<ManagerBaseResponse<bool>> Delete(long id, string userId)
        {
            try
            {
                var entity = await unitOfWork
                               .ProductWarrantiesRepository
                               .GetAsync(entity => entity.Id == id && entity.CreatedBy == userId
                                           && !entity.IsDeleted);
                if (entity != null)
                {
                    entity.IsDeleted = true;
                    entity.DeletedBy = userId;
                    entity.DeletedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }

                var productData = await unitOfWork.ProductsRepository.GetAsync(x => x.Id == entity.ProductId);
                await sendNotificationManager.SendLocationUpdateNotification(new PushNotificationRequestModel()
                {
                    RecipientId = userId,
                    Title = "Warranty information has been deleted",
                    Description = "Warranty information for " + productData.Name + " has been deleted",
                    NotificationType = NotificationType.ProductUpdate,
                    ReferenceId = null
                }, userId);

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Warranty deleted successfully.",
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
        private static string GetStatus(DateTime warrantyEndDate)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan diff = warrantyEndDate.Date - now.Date;
            int months = (warrantyEndDate.Year - now.Year) * 12 + warrantyEndDate.Month - now.Month;
            string status = string.Empty;
            if (diff.Days > 30)
            {
                status = months + " month(s) left";
            }
            else if (diff.Days <= 30 && diff.Days > 0)
            {
                status = diff.Days + " day(s) left";
            }
            else if (diff.Days <= 0)
            {
                status = "Expired";
            }
            return status;
        }
        private async Task<int> WarrantyNameExistsCount(long? id, string warrantyName, string userId)
        {
            if (!string.IsNullOrWhiteSpace(warrantyName))
            {
                var originalWarrantyName = warrantyName;
                var count = 1;

                while (await WarrantyExistsWithName(id, warrantyName, userId))
                {
                    warrantyName = string.Concat(originalWarrantyName, " (", count++, ")");
                }

                return count - 1;
            }

            return 0;
        }
        private async Task<bool> WarrantyExistsWithName(long? id, string warrantyName, string userId)
        {
            return await unitOfWork.ProductWarrantiesRepository
                .GetQueryable(x => !x.IsDeleted && x.CreatedBy == userId
                             && x.Name.ToLower() == warrantyName.ToLower())
                .WhereIf(id != null, x => x.Id != id)
                .AnyAsync();
        }
    }
}
