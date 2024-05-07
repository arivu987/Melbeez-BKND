using Melbeez.Business.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melbeez.Business.Models.UserModels.RequestModels;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IEulaManager
    {
        Task<ManagerBaseResponse<string>> Get();
        Task<ManagerBaseResponse<CookiePolicyRequestModel>> GetEulaForWeb();
        Task<ManagerBaseResponse<CookiePolicyRequestModel>> WrtieInFile(CookiePolicyRequestModel model, string userId);
    }
}
