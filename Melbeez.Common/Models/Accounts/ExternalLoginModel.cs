using System.ComponentModel.DataAnnotations;

namespace Melbeez.Common.Models.Accounts
{
    public class ExternalLoginModel : ExternalLoginBaseModel
    {
        [Required(ErrorMessage = "You must provide a ProviderID")]
        public string ProviderId { get; set; }

        [Required(ErrorMessage = "You must provide a Provider Name")]
        public string Provider { get; set; }
    }
}
