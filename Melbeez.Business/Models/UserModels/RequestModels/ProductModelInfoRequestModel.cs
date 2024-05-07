using Melbeez.Common.Helpers;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ProductModelInfoRequestModel
    {
        public string ModelNumber { get; set; }
        public string ManufacturerName { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ProductName { get; set; }
        public string? CategoryType { get; set; }
        public string? AutomotiveType { get; set; }
        public string? TypeofLicence { get; set; }
        public string? ExpiryDate { get; set; }
        public string? SystemType { get; set; }
        public string? PhoneOS { get; set; }
        public string? Capacity { get; set; }
        public string? NumberOfDoors { get; set; }
        public string? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public string? ManufactureYear { get; set; }
        public string? NoiseLevel { get; set; }
        public string? ControlButtonPlacement { get; set; }
        public string? CookingPower { get; set; }
        public string? Description { get; set; }
        public string? FormBuilderData { get; set; }
        public string? OtherInfo { get; set; }
        public ProductModelStatus Status { get; set; }
    }
}
