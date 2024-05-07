using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IProductCategoriesManager
    {
        Task<ManagerBaseResponse<IEnumerable<ProductCategoriesResponse>>> Get();
        Task<ManagerBaseResponse<IEnumerable<ProductCategoriesFormBuilderResponse>>> GetFormBuilderByCategoryId(long categoryId);
        Task<ManagerBaseResponse<bool>> AddCategoryWithFormBuilder(ProductCategoriesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateCategoryWithFormBuilder(ProductCategoriesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> DeleteCategory(long id, string userId);
        Task<ManagerBaseResponse<IEnumerable<ProductCategoriesResponse>>> GetCategoryForWeb(PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<bool>> AddCategory(ProductCategoriesBaseModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateCategory(ProductCategoriesBaseModel model, string userId);
    }
}
