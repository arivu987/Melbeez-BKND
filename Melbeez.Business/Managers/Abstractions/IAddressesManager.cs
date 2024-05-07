using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IAddressesManager
    {
        Task<ManagerBaseResponse<IEnumerable<AddressResponseModel>>> Get(string userId);
        Task<ManagerBaseResponse<bool>> AddUpdateAddress(List<AddressesRequestModel> model, string userId);
    }
}
