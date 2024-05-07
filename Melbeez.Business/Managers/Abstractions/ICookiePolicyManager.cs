using Melbeez.Business.Models.Common;
using System.Threading.Tasks;
using Melbeez.Business.Models.UserModels.RequestModels;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ICookiePolicyManager
    {
        Task<ManagerBaseResponse<string>> Get();
        Task<ManagerBaseResponse<CookiePolicyRequestModel>> GetCookiePolicyForWeb();
        Task<ManagerBaseResponse<CookiePolicyRequestModel>> WrtieInFile(CookiePolicyRequestModel model, string userId);
    }
}
