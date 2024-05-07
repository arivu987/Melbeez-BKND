using System.Collections.Generic;
using System.Threading.Tasks;
using Melbeez.Business.Models;
using Melbeez.Business.Models.Common;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ICountryManager
    {
        Task<ManagerBaseResponse<IEnumerable<CountryViewModel>>> Get(PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<CountryViewModel>> Get(long id);
        Task<ManagerBaseResponse<bool>> AddCountry(CountriesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateCountry(CountriesRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> DeleteCountry(long Id, string userId);
    }
}
