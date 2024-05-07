using Melbeez.Business.Models.Common;
using Melbeez.Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melbeez.Common.Models;
using System;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IAPIActivitiesManager
    {
        Task<ManagerBaseResponse<IEnumerable<APIActivitiesResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<bool>> Add(APIActivitiesRequestModel model, string userId);
    }
}
