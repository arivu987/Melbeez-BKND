using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ITermsAndConditionsManager
    {
        Task<ManagerBaseResponse<string>> Get();
        Task<ManagerBaseResponse<TermsAndConditionsRequestModel>> GetTermAndConditionsForWeb();
        Task<ManagerBaseResponse<TermsAndConditionsRequestModel>> WrtieInFile(TermsAndConditionsRequestModel model, string userId);
    }
}
