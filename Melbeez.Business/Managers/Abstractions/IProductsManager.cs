using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Common;
using Melbeez.Common.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IProductsManager
    {
        Task<ManagerBaseResponse<IEnumerable<ProductsResponseModel>>> Get(string locationIds, string categoryIds, DateTime? purchaseFromDate, DateTime? purchaseToDate, DateTime? warrantyFromDate, DateTime? warrantyToDate, bool? isTransferItem, PagedListCriteria pagedListCriteria, string userId);
        Task<ManagerBaseResponse<IEnumerable<ProductFormDataRequestModel>>> Get(long id, string userId);
        Task<ManagerBaseResponse<bool>> DeleteProduct(List<long> Ids, string userId);
        Task<ManagerBaseResponse<bool>> MoveProductsLocation(MoveProductsLocationRequestModel model, string userId);
        Task<ManagerBaseResponse<long?>> AddProduct(ProductRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateProduct(long productId, ProductRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> AddProductImages(long productId, List<FileBaseRequest> model, string userId);
        Task<ManagerBaseResponse<bool>> SetDefaultProductImage(long productId, long imageId, string userId);
        Task<ManagerBaseResponse<bool>> DeleteProductImages(List<long> imageids, long productId, string userId);
        Task<ManagerBaseResponse<bool>> MoveProductsToAnotherUserLocation(MoveProductsToAnotherUserLocationRequestModel model, string fromUserId);
        Task<ManagerBaseResponse<bool>> CancelProductMoveRequest(long productId, MovedStatus status, string userId);
    }
}
