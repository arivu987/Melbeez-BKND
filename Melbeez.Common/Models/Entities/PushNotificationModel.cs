using Melbeez.Common.Helpers;
using System;

namespace Melbeez.Common.Models.Entities
{
    public class PushNotificationModel
    {
        public long NotificationId { get; set; }
        public string RecipientId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsSuccess { get; set; }
        public bool IsRead { get; set; }
        public string ReferenceId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ErrorMeassge { get; set; }
        public MovedStatus? Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
