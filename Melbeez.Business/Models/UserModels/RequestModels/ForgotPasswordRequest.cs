using System.ComponentModel.DataAnnotations;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ForgotPasswordRequest
    {
        public string Emailorphone { get; set; }
    }
}
