namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class UserNotificationPreferenceResponseModel
    {
        public long Id { get; set; }
        public bool IsWarrantyExpireAlert { get; set; }
        public bool IsLocationUpdateAlert { get; set; }
        public bool IsProductUpdateAlert { get; set; }
        public bool IsDeviceActivationAlert { get; set; }
        public bool IsMarketingValueAlert { get; set; }
        public bool IsPushNotification { get; set; }
        public bool IsEmailNotification { get; set; }
        public bool IsTextNotification { get; set; }
        public bool IsThirdPartyServiceAllowed { get; set; }
        public bool IsBiometricAllowed { get; set; }
    }
}