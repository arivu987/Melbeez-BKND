using Melbeez.Common.Helpers;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class PushNotificationRequestModel
    {
        public string RecipientId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public NotificationType NotificationType { get; set; }
        public string ReferenceId { get; set; } = null;
        public MovedStatus? Status { get; set; } = null;
    }
}
