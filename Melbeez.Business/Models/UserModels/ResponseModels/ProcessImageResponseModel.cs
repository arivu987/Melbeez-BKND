using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ProcessImageResponseModel
    {
        public List<string> BrandNames { get; set; }
        public List<string> ModelNumbers { get; set; }
        public List<string> SerialNumbers { get; set; }
    }
}
