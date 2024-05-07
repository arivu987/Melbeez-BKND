namespace Melbeez.Business.Models.Common
{
    public class PageDetailModel
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int Count { get; set; }
        public string SearchText { get; set; }
    }
    public class NotificationPageDetailModel : PageDetailModel
    {
        public int UnreadNotificationCount { get; set; }
    }
}
