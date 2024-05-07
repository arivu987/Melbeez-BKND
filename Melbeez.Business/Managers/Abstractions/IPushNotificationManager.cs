using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IPushNotificationManager
    {
        Task<ManagerBaseResponse<IEnumerable<PushNotificationModel>>> Get(PagedListCriteria pagedListCriteria, string userId);
        Task Add(PushNotificationModel model, string senderUserId);
        Task<ManagerBaseResponse<bool>> ReadNotification(PushNotificationReadRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> DeleteNotification(List<long?> ids, string userId);
    }
}
