using System.ComponentModel.DataAnnotations;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class UserUpdateModel
    {
        [MaxLength(256)]
        public string FirstName { get; set; }
        [MaxLength(256)]
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "The Email Address is not valid.")]
        public string Email { get; set; }
        public string CountryCode { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        public string UserName { get; set; }
        public string ProfileUrl { get; set; }
        public string CurrencyCode { get; set; }
    }
}
