using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IStatesManager
    {
        Task<ManagerBaseResponse<IEnumerable<StatesResponseModel>>> Get(PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<StatesResponseModel>> GetStateById(long id);
        Task<ManagerBaseResponse<IEnumerable<StatesResponseModel>>> GetStateByCountryId(long countryId);
        Task<ManagerBaseResponse<bool>> AddState(StatesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateState(StatesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> DeleteState(long Id, string userId);
    }
}
