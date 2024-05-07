using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ISendNotificationManager
    {
        Task<ManagerBaseResponse<bool>> SendWarrentyExpiryNotification(string senderUserId);
        Task<ManagerBaseResponse<bool>> SendLocationUpdateNotification(PushNotificationRequestModel model, string senderUserId);
        Task<ManagerBaseResponse<bool>> SendProductUpdateNotification(PushNotificationRequestModel model, string senderUserId);
        Task<ManagerBaseResponse<bool>> SendDeviceActivationNotification(PushNotificationRequestModel model, string senderUserId);
        Task<ManagerBaseResponse<bool>> SendMarketingAlertNotification(PushNotificationRequestModel model, string senderUserId);
        Task<ManagerBaseResponse<bool>> SendItemTransferNotification(PushNotificationRequestModel model, string userId);

    }
}
