using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IPrivacyPolicyManager
    {
        Task<ManagerBaseResponse<string>> Get();
        Task<ManagerBaseResponse<PrivacyPolicyRequestModel>> GetPrivacyPolicyForWeb();
        Task<ManagerBaseResponse<PrivacyPolicyRequestModel>> WrtieInFile(PrivacyPolicyRequestModel model, string userId);
    }
}
