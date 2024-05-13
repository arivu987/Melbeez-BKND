using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IProductModelInformationManager
    {
        Task<ManagerBaseResponse<List<ProductModelInfoResponse>>> GetProductsModelInfo(string modelNumber, string manufacturerName);
        Task<ManagerBaseResponse<IEnumerable<ProductModelInfoResponse>>> Get(ProductModelStatus? status, PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<bool>> UpdateProductModelStatus(List<long> ids, ProductModelStatus status, string userId);
        Task<ManagerBaseResponse<bool>> DeleteProductModelInfo(long Id, string userId);
        Task<ManagerBaseResponse<bool>> AddProductModelInfo(ProductModelInfoRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateProductModelInfo(long id, ProductModelInfoRequestModel model, string userId);
        Task<ManagerBaseResponse<List<BulkUploadInsertedRowStatusResponseModel>>> UploadBulkProductModels(IFormFile file, string userId);
        Task<ManagerBaseResponse<bool>> IsModelAndManufacturerAlreadyExists(string manufacturerName, string modelNumber);
        Task<ManagerBaseResponse<IEnumerable<ManufacturerModelInfoResponse>>> GetManufacturerModelNumber(string manufacturerName);
        Task<ManagerBaseResponse<ProcessImageResponseModel>> ProcessImageWithOCR(IFormFile imageFile);
    }
}
