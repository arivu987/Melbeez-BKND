namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class PushNotificationReadRequestModel
    {
        public long Id { get; set; }
        public bool IsRead { get; set; }
        public bool IsReadAll { get; set; }
    }
}
