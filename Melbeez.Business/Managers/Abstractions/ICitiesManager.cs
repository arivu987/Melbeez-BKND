using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ICitiesManager
    {
        Task<ManagerBaseResponse<IEnumerable<CitiesResponseModel>>> Get(PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<CitiesResponseModel>> GetCityById(long id);
        Task<ManagerBaseResponse<IEnumerable<CitiesResponseModel>>> GetCityByStateId(long stateId);
        Task<ManagerBaseResponse<CitiesResponseModel>> AddCity(CitiesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateCity(CitiesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> DeleteCity(long Id, string userId);
    }
}
