using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IUserNotificationPreferenceManager
	{
		Task<ManagerBaseResponse<UserNotificationPreferenceResponseModel>> Get(string userId);
		Task<ManagerBaseResponse<bool>> AddUserNotificationPreference(UserNotificationPreferenceResponseModel model, string userId);
		Task<ManagerBaseResponse<bool>> UpdateUserNotificationPreference(UserNotificationPreferenceResponseModel model, string userId);
	}
}