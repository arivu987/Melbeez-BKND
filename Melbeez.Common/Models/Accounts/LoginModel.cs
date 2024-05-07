using System.ComponentModel.DataAnnotations;

namespace Melbeez.Common.Models.Accounts
{
    public class LoginModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
