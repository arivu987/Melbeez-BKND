using System.ComponentModel.DataAnnotations;

namespace Melbeez.Common.Models.Accounts
{
    public class RegisterBaseModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "The Email Address is not valid.")]
        public string Email { get; set; }
        [Required]
        public string CountryCode { get; set; }
        [Required]
        [MaxLength(10, ErrorMessage = "Mobile number is not vaild.")]
        public string PhoneNumber { get; set; }
    }

    public class RegisterModel : RegisterBaseModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
    public class ExternalLoginBaseModel
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "The Email Address is not valid.")]
        public string Email { get; set; }
        [MaxLength(10, ErrorMessage = "Mobile number is not vaild.")]
        public string PhoneNumber { get; set; }
        public string ProfileUrl { get; set; }
    }
}
