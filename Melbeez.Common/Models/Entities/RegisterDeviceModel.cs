namespace Melbeez.Common.Models.Entities
{
    public class RegisterDeviceModel
    {
        public long Id { get; set; }
        public string DeviceToken { get; set; }
        public string UId { get; set; }
        public int DeviceType { get; set; }
        public string AppVersion { get; set; }
        public string OSVersion { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
