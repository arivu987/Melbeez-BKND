using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ProductCategoriesManager : IProductCategoriesManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IWebHostEnvironment environment;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public ProductCategoriesManager(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
        {
            this.unitOfWork = unitOfWork;
            this.environment = environment;
        }

        public async Task<ManagerBaseResponse<IEnumerable<ProductCategoriesResponse>>> Get()
        {
            var result = await unitOfWork
                .ProductCategoriesRepository
                .GetQueryable(x => !x.IsDeleted && x.IsActive)
                .Select(s => new ProductCategoriesResponse()
                {
                    CategoryId = s.Id,
                    CategoryName = s.Name,
                    IsActive = s.IsActive,
                    Sequence = s.Sequence
                })
                .OrderBy(o => o.Sequence)
                .AsNoTracking()
                .ToListAsync();
            return new ManagerBaseResponse<IEnumerable<ProductCategoriesResponse>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<IEnumerable<ProductCategoriesResponse>>> GetCategoryForWeb(PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .ProductCategoriesRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(s => new ProductCategoriesResponse()
                {
                    CategoryId = s.Id,
                    CategoryName = s.Name,
                    IsActive = s.IsActive,
                    Sequence = s.Sequence
                })
                .OrderBy(o => o.Sequence)
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.CategoryName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<ProductCategoriesResponse>>()
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
        public async Task<ManagerBaseResponse<IEnumerable<ProductCategoriesFormBuilderResponse>>> GetFormBuilderByCategoryId(long categoryId)
        {
            var result = await unitOfWork
                .ProductCategoriesRepository
                .GetQueryable(x => !x.IsDeleted && x.Id == categoryId && x.IsActive)
                .Select(s => new ProductCategoriesFormBuilderResponse()
                {
                    CategoryId = s.Id,
                    CategoryName = s.Name,
                    FormBuilderData = s.FormBuilderData,
                    Sequence = s.Sequence,
                    IsActive = s.IsActive
                })
                .AsNoTracking()
                .ToListAsync();
            return new ManagerBaseResponse<IEnumerable<ProductCategoriesFormBuilderResponse>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddCategoryWithFormBuilder(ProductCategoriesRequestModel model, string userId)
        {
            try
            {
                if (model.CategoryId == 0)
                {
                    if (model.CategoryName.ToLower().Equals("other") || model.CategoryName.ToLower().Equals("others"))
                    {
                        model.Sequence = 99999;
                    }
                    await unitOfWork.ProductCategoriesRepository.AddAsync(new ProductCategoriesEntity()
                    {
                        Id = model.CategoryId,
                        Name = model.CategoryName,
                        FormBuilderData = JsonConvert.SerializeObject(model.FormBuilderData),
                        Sequence = model.Sequence,
                        IsActive = model.IsActive,
                        IsDeleted = false,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = userId,
                        UpdatedOn = DateTime.UtcNow
                    });
                    await unitOfWork.CommitAsync();
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Category added successfully.",
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
        public async Task<ManagerBaseResponse<bool>> UpdateCategoryWithFormBuilder(ProductCategoriesRequestModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                    .ProductCategoriesRepository
                    .GetAsync(entity => entity.Id == model.CategoryId
                                && !entity.IsDeleted);
                if (entity != null)
                {
                    entity.Name = model.CategoryName;
                    entity.FormBuilderData = JsonConvert.SerializeObject(model.FormBuilderData);
                    entity.Sequence = model.Sequence;
                    entity.IsActive = model.IsActive;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Category updated successfully.",
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
        public async Task<ManagerBaseResponse<bool>> DeleteCategory(long id, string userId)
        {
            try
            {
                var productCount = unitOfWork
                                  .ProductsRepository
                                  .GetQueryable(entity => entity.CategoryId == id && !entity.IsDeleted)
                                  .ToList()
                                  .Count();

                if (productCount > 0)
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "The category cannot be deleted because some products are associated to it.",
                        Result = false,
                        StatusCode = 500
                    };
                }

                var entity = await unitOfWork
                            .ProductCategoriesRepository
                            .GetAsync(entity => entity.Id == id && !entity.IsDeleted);

                if (entity != null)
                {
                    entity.IsDeleted = true;
                    entity.DeletedBy = userId;
                    entity.DeletedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();

                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Category deleted successfully.",
                        Result = true
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Provide valid category id.",
                    Result = false,
                    StatusCode = 500
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
        public async Task<ManagerBaseResponse<bool>> AddCategory(ProductCategoriesBaseModel model, string userId)
        {
            if (!string.IsNullOrEmpty(model.CategoryName) && model.CategoryName != "undefined")
            {
                var otherCategoryFilePath = Path.Combine(environment.WebRootPath, "CategoryJson/Others.json");
                var formBuilderData = JsonConvert.DeserializeObject(File.ReadAllText(otherCategoryFilePath));
                var categoriesData = unitOfWork.ProductCategoriesRepository.GetQueryable(x => !x.IsDeleted);
                var isCategoryExists = categoriesData.Any(x => !x.IsDeleted && x.Name.ToLower().Equals(model.CategoryName.ToLower()));
                if (!isCategoryExists)
                {
                    var seq = categoriesData.Where(x => x.Sequence != 99999).Max(x => x.Sequence);
                    await unitOfWork.ProductCategoriesRepository.AddAsync(new ProductCategoriesEntity()
                    {
                        Name = model.CategoryName,
                        FormBuilderData = JsonConvert.SerializeObject(formBuilderData),
                        Sequence = seq + 1,
                        IsActive = model.IsActive,
                        IsDeleted = false,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedBy = userId,
                        UpdatedOn = DateTime.UtcNow
                    });
                    await unitOfWork.CommitAsync();

                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Category added successfully.",
                        Result = true
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Category already exists.",
                    Result = false
                };
            }
            else
            {
                return new ManagerBaseResponse<bool>()
                {
                    Result = false,
                    Message = "Please provide vaild category name",
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateCategory(ProductCategoriesBaseModel model, string userId)
        {
            var entity = await unitOfWork
                        .ProductCategoriesRepository
                        .GetAsync(entity => entity.Id == model.CategoryId && !entity.IsDeleted);

            if (entity != null)
            {
                entity.Name = model.CategoryName;
                entity.IsActive = model.IsActive;
                entity.UpdatedBy = userId;
                entity.UpdatedOn = DateTime.UtcNow;
                await unitOfWork.CommitAsync();

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Category updated successfully.",
                    Result = true
                };
            }
            return new ManagerBaseResponse<bool>()
            {
                Message = "Category not found.",
                Result = false,
                StatusCode = 404
            };
        }
    }
}
