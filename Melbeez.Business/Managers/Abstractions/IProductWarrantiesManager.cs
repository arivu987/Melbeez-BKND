using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IProductWarrantiesManager
    {
        Task<ManagerBaseResponse<IEnumerable<BaseWarrantiesResponseModel>>> Get(WarrantyStatus? status, string categories, DateTime? warrantyFromDate, DateTime? warrantyToDate, PagedListCriteria pagedListCriteria, string userId);
        Task<ManagerBaseResponse<IEnumerable<ProductWarrantiesResponseModel>>> Get(long productId, long? id, string userId);
        Task<ManagerBaseResponse<bool>> AddWarranty(List<ProductWarrantiesRequestModel> model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateWarranty(List<ProductWarrantiesRequestModel> model, string userId);
        Task<ManagerBaseResponse<bool>> Delete(long id, string userId);
    }
}
