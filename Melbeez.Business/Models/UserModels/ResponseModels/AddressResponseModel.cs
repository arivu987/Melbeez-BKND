using Melbeez.Common.Helpers;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class AddressResponseModel
    {
        public long Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string ZipCode { get; set; }
        public AddressType TypeOfProperty { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSameMailingAddress { get; set; }
    }
}
