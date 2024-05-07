using Melbeez.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Melbeez.Common.Models.Accounts
{
    public class UserResponseBaseModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string CountyCode { get; set; }
        public string CurrencyCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string ProfileUrl { get; set; }
        public bool? IsTemporaryLockOut { get; set; }
        public bool? IsPermanentLocuOut { get; set; }
        public bool? IsBlockedByAdmin { get; set; }
        public bool? IsVerifiedByAdmin { get; set; }
        public DateTime CreateOn { get; set; }
    }
    public class UserResponseModel : UserResponseBaseModel
    {
        public List<UserAddressesModel> UserAddresses { get; set; }
    }
    public class UserAddressesModel
    {
        public long AddressId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string ZipCode { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public AddressType TypeOfProperty { get; set; }
        public bool IsDefault { get; set; }
        public bool IsSameMailingAddress { get; set; }
    }
}
