using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using Melbeez.Business.Common.Services;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ProductModelInformationManager : IProductModelInformationManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IConfiguration configuration;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        private readonly UserManager<ApplicationUser> userManager;

        public ProductModelInformationManager(IUnitOfWork unitOfWork,
                               IConfiguration configuration,
                               UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.configuration = configuration;
            this.userManager = userManager;
        }
        public async Task<ManagerBaseResponse<List<ProductModelInfoResponse>>> GetProductsModelInfo(string modelNumber, string manufacturerName)
        {
            try
            {
                var result = await unitOfWork.ProductModelInformationRepository
                            .GetQueryable(x => !x.IsDeleted && x.Status == ProductModelStatus.Approved)
                            .WhereIf(!string.IsNullOrEmpty(manufacturerName) && string.IsNullOrEmpty(modelNumber), x => x.ManufacturerName.ToLower().Contains(manufacturerName.ToLower()))
                            .WhereIf(string.IsNullOrEmpty(manufacturerName) && !string.IsNullOrEmpty(modelNumber), x => x.ModelNumber.ToLower().Contains(modelNumber.ToLower()))
                            .WhereIf(!string.IsNullOrEmpty(manufacturerName) && !string.IsNullOrEmpty(modelNumber), x => x.ManufacturerName.ToLower().Contains(manufacturerName.ToLower()) && x.ModelNumber.ToLower().Contains(modelNumber.ToLower()))
                            .Select(x => new ProductModelInfoResponse()
                            {
                                Id = x.Id,
                                ModelNumber = x.ModelNumber,
                                ManufacturerName = x.ManufacturerName,
                                CategoryId = x.CategoryId,
                                CategoryName = x.CategoryName,
                                ProductName = x.ProductName,
                                CategoryType = x.CategoryType,
                                AutomotiveType = x.AutomotiveType,
                                TypeofLicence = x.TypeofLicence,
                                ExpiryDate = x.ExpiryDate,
                                SystemType = x.SystemType,
                                PhoneOS = x.PhoneOS,
                                Capacity = x.Capacity,
                                NumberOfDoors = x.NumberOfDoors,
                                ScreenSize = x.ScreenSize,
                                Resolution = x.Resolution,
                                ManufactureYear = x.ManufactureYear,
                                NoiseLevel = x.NoiseLevel,
                                ControlButtonPlacement = x.ControlButtonPlacement,
                                CookingPower = x.CookingPower,
                                Description = x.Description,
                                FormBuilderData = x.FormBuilderData,
                                OtherInfo = x.OtherInfo,
                                Status = x.Status,
                                IsDraft = x.IsDraft,
                                CreatedBy = x.CreatedBy,
                                UpdatedBy = x.UpdatedBy
                            }).ToListAsync();
                foreach (var item in result)
                {
                    item.CreatedBy = GetUserName(item.CreatedBy);
                    item.UpdatedBy = GetUserName(item.UpdatedBy);
                }
                return new ManagerBaseResponse<List<ProductModelInfoResponse>>()
                {
                    Message = "Product Model Information get sucessfully",
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<List<ProductModelInfoResponse>>()
                {
                    Message = ex.Message,
                    Result = new List<ProductModelInfoResponse>()
                };
            }
        }
        public async Task<ManagerBaseResponse<IEnumerable<ProductModelInfoResponse>>> Get(ProductModelStatus? status, PagedListCriteria pagedListCriteria)
        {
            try
            {
                var result = await unitOfWork.ProductModelInformationRepository
                            .GetQueryable(x => !x.IsDeleted)
                            .WhereIf(status.HasValue, x => x.Status == status)
                            .Select(x => new ProductModelInfoResponse()
                            {
                                Id = x.Id,
                                ModelNumber = x.ModelNumber,
                                ManufacturerName = x.ManufacturerName,
                                CategoryId = x.CategoryId,
                                CategoryName = x.CategoryName,
                                ProductName = x.ProductName,
                                CategoryType = x.CategoryType,
                                AutomotiveType = x.AutomotiveType,
                                TypeofLicence = x.TypeofLicence,
                                ExpiryDate = x.ExpiryDate,
                                SystemType = x.SystemType,
                                PhoneOS = x.PhoneOS,
                                Capacity = x.Capacity,
                                NumberOfDoors = x.NumberOfDoors,
                                ScreenSize = x.ScreenSize,
                                Resolution = x.Resolution,
                                ManufactureYear = x.ManufactureYear,
                                NoiseLevel = x.NoiseLevel,
                                ControlButtonPlacement = x.ControlButtonPlacement,
                                CookingPower = x.CookingPower,
                                Description = x.Description,
                                FormBuilderData = x.FormBuilderData,
                                OtherInfo = !string.IsNullOrEmpty(x.OtherInfo) ? FormatJsonString(x.OtherInfo) : x.OtherInfo,
                                Status = x.Status,
                                IsDraft = x.IsDraft,
                                CreatedBy = x.CreatedBy,
                                CreatedOn = x.CreatedOn,
                                UpdatedBy = x.UpdatedBy
                            })
                            .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.ModelNumber.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                    || x.ManufacturerName.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                    || (!string.IsNullOrEmpty(x.CategoryName) && x.CategoryName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                                                    || (!string.IsNullOrEmpty(x.ProductName) && x.ProductName.ToLower().Contains(pagedListCriteria.SearchText.ToLower())))
                            .AsNoTracking()
                            .ToPagedListAsync(pagedListCriteria, orderByTranslations);

                foreach (var item in result.Data)
                {
                    item.CreatedBy = GetUserName(item.CreatedBy);
                    item.UpdatedBy = GetUserName(item.UpdatedBy);
                }

                return new ManagerBaseResponse<IEnumerable<ProductModelInfoResponse>>
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
            catch (Exception ex)
            {
                return new ManagerBaseResponse<IEnumerable<ProductModelInfoResponse>>()
                {
                    Message = ex.Message,
                    Result = null
                };
            }
        }
        public static string FormatJsonString(string jsonString)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<JObject>(jsonString);
                var formattedString = string.Join("; ", json.Properties().Select(p => $"{p.Name}: {p.Value}"));
                formattedString = string.Concat(formattedString, ";");
                return formattedString;
            }
            catch (JsonException)
            {
                return jsonString;
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateProductModelStatus(List<long> ids, ProductModelStatus status, string userId)
        {
            try
            {
                foreach (var id in ids)
                {
                    var productModelInfo = await unitOfWork.ProductModelInformationRepository
                        .GetAsync(entity => entity.Id == id && !entity.IsDeleted);
                    if (productModelInfo != null)
                    {
                        if (status == ProductModelStatus.Submitted && productModelInfo.IsDraft)
                        {
                            productModelInfo.IsDraft = false;
                        }
                        productModelInfo.Status = status;
                        productModelInfo.UpdatedOn = DateTime.UtcNow;
                        productModelInfo.UpdatedBy = userId;
                        await unitOfWork.CommitAsync();
                    }
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product Model(s) " + status.ToString().ToLower() + " successfully.",
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
        public async Task<ManagerBaseResponse<bool>> DeleteProductModelInfo(long Id, string userId)
        {
            try
            {
                var entity = await unitOfWork
                            .ProductModelInformationRepository
                            .GetAsync(entity => entity.Id == Id && !entity.IsDeleted);
                if (entity != null)
                {
                    entity.IsDeleted = true;
                    entity.DeletedOn = DateTime.UtcNow;
                    entity.DeletedBy = userId;
                    await unitOfWork.CommitAsync();
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product information delete successfully.",
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
        public async Task<ManagerBaseResponse<bool>> AddProductModelInfo(ProductModelInfoRequestModel model, string userId)
        {
            try
            {
                var isProductExist = await unitOfWork.ProductModelInformationRepository
                                 .AnyAsync(x => !x.IsDeleted && x.ModelNumber.ToLower() == model.ModelNumber.ToLower()
                                             && x.ManufacturerName.ToLower() == model.ManufacturerName.ToLower());
                if (isProductExist)
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Manufacturer Name & Model Number already exists! (Manufacturer Name : " + model.ManufacturerName + " Model Number : " + model.ModelNumber + ")",
                        Result = false
                    };
                }

                if (!string.IsNullOrEmpty(model.OtherInfo))
                {
                    Dictionary<string, string> rowDict = new Dictionary<string, string>();
                    ParseAndPopulateDict(model.OtherInfo, rowDict);
                    model.OtherInfo = JsonConvert.SerializeObject(rowDict);
                }

                var productmodel = await unitOfWork.ProductModelInformationRepository
                                .AddAsync(new ProductModelInformationEntity()
                                {
                                    ModelNumber = model.ModelNumber,
                                    ManufacturerName = model.ManufacturerName,
                                    CategoryId = model.CategoryId,
                                    CategoryName = model.CategoryName,
                                    ProductName = model.ProductName,
                                    OtherInfo = model.OtherInfo,
                                    Status = ProductModelStatus.Pendding,
                                    IsDraft = true,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow,
                                    IsDeleted = false,
                                });

                await unitOfWork.CommitAsync();

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product information added successfully.",
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
        public async Task<ManagerBaseResponse<bool>> UpdateProductModelInfo(long id, ProductModelInfoRequestModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                        .ProductModelInformationRepository
                        .GetAsync(entity => !entity.IsDeleted && entity.Id == id);
                if (entity != null)
                {
                    if (!string.IsNullOrEmpty(model.OtherInfo))
                    {
                        Dictionary<string, string> rowDict = new Dictionary<string, string>();
                        ParseAndPopulateDict(model.OtherInfo, rowDict);
                        model.OtherInfo = JsonConvert.SerializeObject(rowDict);
                    }
                    entity.ModelNumber = model.ModelNumber;
                    entity.ManufacturerName = model.ManufacturerName;
                    entity.CategoryId = model.CategoryId;
                    entity.CategoryName = model.CategoryName;
                    entity.ProductName = model.ProductName;
                    entity.OtherInfo = model.OtherInfo;
                    entity.Status = ProductModelStatus.Pendding;
                    entity.IsDraft = true;
                    entity.UpdatedOn = DateTime.UtcNow;
                    entity.UpdatedBy = userId;

                    var productInfoList = await unitOfWork
                        .ProductsRepository
                        .GetQueryable(prod => !prod.IsDeleted && prod.ProductModelInfoId == id)
                        .ToListAsync();

                    if (productInfoList.Any())
                    {
                        foreach (var item in productInfoList)
                        {
                            item.Name = model.ProductName;
                            item.ModelNumber = model.ModelNumber;
                            item.ManufactureName = model.ManufacturerName;
                            item.CategoryId = model.CategoryId;
                            item.OtherInfo = model.OtherInfo;
                            item.UpdatedBy = userId;
                            item.UpdatedOn = DateTime.UtcNow;
                            await unitOfWork.CommitAsync();
                        }
                    }

                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Product information approved successfully.",
                        Result = true
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Product model Information not found.",
                    Result = false
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
        public async Task<ManagerBaseResponse<List<BulkUploadInsertedRowStatusResponseModel>>> UploadBulkProductModels(IFormFile file, string userId)
        {
            try
            {
                List<BulkUploadInsertedRowStatusResponseModel> uploadStatusList = new List<BulkUploadInsertedRowStatusResponseModel>();
                var fileExtension = configuration.GetValue<string>("ValidFileExtensions").Split(",").ToList();
                string FileEx = System.IO.Path.GetExtension(file.FileName);
                if (!fileExtension.Contains(FileEx))
                {
                    return new ManagerBaseResponse<List<BulkUploadInsertedRowStatusResponseModel>>()
                    {
                        Result = null,
                        Message = "The file is not an excel file!"
                    };
                }

                var userInfo = userManager.Users.Where(x => !x.IsDeleted && x.Id == userId).FirstOrDefault();
                var role = await userManager.GetRolesAsync(userInfo);

                byte[] byteArray = null;
                using (var br = new BinaryReader(file.OpenReadStream()))
                {
                    byteArray = br.ReadBytes((int)file.Length);
                }

                DataTable dt = ExcelService.GetDataTableBulkImport(byteArray);
                dt.Rows.RemoveAt(0);
                if (dt.Rows.Count > 0)
                {
                    int rowCount = 2;
                    foreach (DataRow row in dt.Rows)
                    {
                        if (!string.IsNullOrWhiteSpace(row["Category"].ToString()) && !string.IsNullOrWhiteSpace(row["Model Number"].ToString()) && !string.IsNullOrWhiteSpace(row["Manufacture Name"].ToString()))
                        {
                            var categoryInfo = await unitOfWork
                                            .ProductCategoriesRepository
                                            .GetAsync(x => !x.IsDeleted
                                                        && x.Name.ToLower() == row["Category"].ToString().ToLower());

                            if (categoryInfo == null)
                            {
                                uploadStatusList.Add(new BulkUploadInsertedRowStatusResponseModel()
                                {
                                    RowNumber = rowCount.ToString(),
                                    Status = "Failed",
                                    Message = "Category name '" + row["Category"].ToString() + "' doesn't match our records!"
                                });
                                rowCount++;
                                continue;
                            }

                            var isExits = await unitOfWork.ProductModelInformationRepository
                                        .GetQueryable(x => !x.IsDeleted && x.ModelNumber.ToLower() == row["Model Number"].ToString().ToLower()
                                                        && x.ManufacturerName.ToLower() == row["Manufacture Name"].ToString().ToLower())
                                        .AnyAsync();
                            if (isExits)
                            {
                                uploadStatusList.Add(new BulkUploadInsertedRowStatusResponseModel()
                                {
                                    RowNumber = rowCount.ToString(),
                                    Status = "Failed",
                                    Message = "Manufacture name '" + row["Manufacture Name"].ToString() + "' and Model Number '" + row["Model Number"].ToString() + "' already exists!"
                                });
                                rowCount++;
                                continue;
                            }

                            #region Get OtherInfo data and convert it into proper JSON format
                            string otherInfo = string.Empty;
                            if (!string.IsNullOrEmpty(row["Other Info"].ToString()))
                            {
                                Dictionary<string, string> rowDict = new();
                                ParseAndPopulateDict(row["Other Info"].ToString(), rowDict);
                                otherInfo = JsonConvert.SerializeObject(rowDict);
                            }
                            #endregion

                            var productInfoModel = await unitOfWork.ProductModelInformationRepository
                                .AddAsync(new ProductModelInformationEntity()
                                {
                                    CategoryId = categoryInfo.Id,
                                    CategoryName = row["Category"].ToString(),
                                    ModelNumber = row["Model Number"].ToString(),
                                    ManufacturerName = row["Manufacture Name"].ToString(),
                                    ProductName = row["Product Name"].ToString(),
                                    FormBuilderData = null,
                                    OtherInfo = otherInfo,
                                    Status = role.Contains("SuperAdmin") ? ProductModelStatus.Approved : ProductModelStatus.Pendding,
                                    IsDraft = true,
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = userId,
                                    UpdatedOn = DateTime.UtcNow,
                                    IsDeleted = false,
                                });
                            await unitOfWork.CommitAsync();
                            uploadStatusList.Add(new BulkUploadInsertedRowStatusResponseModel()
                            {
                                RowNumber = rowCount.ToString(),
                                Status = "Success",
                                Message = "Manufacture name '" + row["Manufacture Name"].ToString() + "' and Model Number '" + row["Model Number"].ToString() + "' inserted successfully"
                            });
                        }
                        else
                        {
                            uploadStatusList.Add(new BulkUploadInsertedRowStatusResponseModel()
                            {
                                RowNumber = rowCount.ToString(),
                                Status = "Failed",
                                Message = "Category, Model Number and Manufacture Name are not allow as White Space or Null or Empty String!"
                            });
                            rowCount++;
                            continue;
                        }
                        rowCount++;
                    }
                    return new ManagerBaseResponse<List<BulkUploadInsertedRowStatusResponseModel>>()
                    {
                        Message = "Product information added successfully.",
                        Result = uploadStatusList
                    };
                }
                else
                {
                    return new ManagerBaseResponse<List<BulkUploadInsertedRowStatusResponseModel>>()
                    {
                        Message = "The file doesn't have any data!",
                        Result = null
                    };
                }
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<List<BulkUploadInsertedRowStatusResponseModel>>()
                {
                    Message = ex.Message,
                    Result = null
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> IsModelAndManufacturerAlreadyExists(string manufacturerName, string modelNumber)
        {
            var isProductExist = await unitOfWork.ProductModelInformationRepository
                                .AnyAsync(x => !x.IsDeleted && x.ModelNumber.ToLower() == modelNumber.ToLower()
                                            && x.ManufacturerName.ToLower() == manufacturerName.ToLower());
            if (isProductExist)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Manufacturer Name & Model Number already exists! (Manufacturer Name : " + manufacturerName + " Model Number : " + modelNumber + ")",
                    Result = true
                };
            }
            return new ManagerBaseResponse<bool>()
            {
                Result = false
            };
        }
        public string GetUserName(string userId)
        {
            var firstName = userManager.Users.Where(x => x.Id == userId).FirstOrDefault()?.FirstName;
            var lastName = userManager.Users.Where(x => x.Id == userId).FirstOrDefault()?.LastName;
            return string.Concat(firstName, " ", lastName);
        }
        public static void ParseAndPopulateDict(string rowData, Dictionary<string, string> dataDict)
        {
            string[] keyValuePairs = rowData.Split(';');
            foreach (string keyValuePair in keyValuePairs)
            {
                string[] parts = keyValuePair.Split(':');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    dataDict[key] = value;
                }
            }
        }
        public async Task<ManagerBaseResponse<IEnumerable<ManufacturerModelInfoResponse>>> GetManufacturerModelNumber(string manufacturerName)
        {
            var result = await unitOfWork.ProductModelInformationRepository
                        .GetQueryable(x => !x.IsDeleted && x.Status == ProductModelStatus.Approved
                                        && x.ManufacturerName.ToLower().Contains(manufacturerName.ToLower())
                        )
                        .GroupBy(x => x.ManufacturerName)
                        .Select(group => new ManufacturerModelInfoResponse()
                        {
                            ManufacturerName = group.Key,
                            ModelNumber = new HashSet<string>(group.Select(x => x.ModelNumber))
                        })
                        .ToListAsync();

            return new ManagerBaseResponse<IEnumerable<ManufacturerModelInfoResponse>>()
            {
                Result = result
            };
        }
    }
}
