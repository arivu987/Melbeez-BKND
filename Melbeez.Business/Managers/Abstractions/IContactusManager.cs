using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IContactusManager
    {
        Task<ManagerBaseResponse<IEnumerable<ContactusResponseModel>>> Get(PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<bool>> AddContactusData(ContactusRequestModel model, string userId);
    }
}
