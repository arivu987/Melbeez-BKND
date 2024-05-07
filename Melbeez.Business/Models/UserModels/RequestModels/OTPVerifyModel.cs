using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class OTPVerifyModel
    {
        public string Otp { get; set; }
        public string Emailorphone { get; set; }
        public string purpose { get; set; }
    }
}
