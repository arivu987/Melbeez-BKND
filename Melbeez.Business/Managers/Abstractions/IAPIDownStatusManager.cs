using Melbeez.Business.Models.Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melbeez.Common.Models;
using Melbeez.Business.Models.UserModels.ResponseModels;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IAPIDownStatusManager
    {
        Task<ManagerBaseResponse<IEnumerable<APIDownStatusResponseModel>>> Get(PagedListCriteria pagedListCriteria);
        Task<bool> GetLastApiStatusData();
        Task<ManagerBaseResponse<bool>> Add(bool isAPIDown, string userId);
    }
}
