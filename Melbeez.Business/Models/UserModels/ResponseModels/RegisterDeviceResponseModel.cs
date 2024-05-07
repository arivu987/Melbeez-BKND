using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class RegisterDeviceResponseModel
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string DeviceToken { get; set; }
        public string UId { get; set; }
        public int DeviceType { get; set; }
        public string AppVersion { get; set; }
        public string OSVersion { get; set; }
        public double Lat { get; set; }
        public double Long { get; set; }
    }
}
