using System.ComponentModel.DataAnnotations;

namespace Melbeez.Common.Models.Accounts
{
    public class RefreshTokenModel
    {
        [Required(ErrorMessage = "You must provide a Token")]
        public string Token { get; set; }
        [Required(ErrorMessage = "You must provide a RefreshToken")]
        public string RefreshToken { get; set; }
    }
}
