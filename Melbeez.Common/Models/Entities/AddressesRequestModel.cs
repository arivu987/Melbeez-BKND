using Melbeez.Common.Helpers;

namespace Melbeez.Common.Models.Entities
{
    public class AddressesRequestModel
    {
        public long Id { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string Image { get; set; }
        public AddressType TypeOfProperty { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSameMailingAddress { get; set; }
    }
}
