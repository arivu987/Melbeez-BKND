using System.ComponentModel.DataAnnotations;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ForgotPasswordVerifyRequest
    {
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string Confirmpassword { get; set; } = null!;
        public string Emailorphone { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
