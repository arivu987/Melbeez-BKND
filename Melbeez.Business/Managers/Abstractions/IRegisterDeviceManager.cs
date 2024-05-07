using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models.Entities;
using Melbeez.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IRegisterDeviceManager
    {
        Task<ManagerBaseResponse<IEnumerable<RegisterDeviceResponseModel>>> Get();
        Task<ManagerBaseResponse<IEnumerable<RegisterDeviceResponseModel>>> Get(string uid, string userId);
        Task<ManagerBaseResponse<bool>> AddRegisterDevice(RegisterDeviceModel model, string userId);
        Task<ManagerBaseResponse<bool>> DeleteRegisterDevice(string uid, string userId);
    }
}