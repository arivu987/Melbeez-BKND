using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ILocationsManager
    {
        Task<ManagerBaseResponse<IEnumerable<LocationsResponseModel>>> Get(string locationTypes, string categoryId, DateTime? from, DateTime? to, PagedListCriteria pagedListCriteria, string userId);
        Task<ManagerBaseResponse<LocationsResponseModel>> Get(long id, string userId);
        Task<ManagerBaseResponse<LocationsEntity>> AddLocation(LocationsRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> UpdateLocation(LocationsRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> DeleteLocation(List<long> Id, string userId, string callingFrom = null);
        Task<ManagerBaseResponse<IEnumerable<LocationsResponseModel>>> GetLocationByUserId(string userId);
    }
}
