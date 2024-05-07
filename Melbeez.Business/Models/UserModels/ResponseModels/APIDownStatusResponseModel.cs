using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class APIDownStatusResponseModel
    {
        public long Id { get; set; }
        public bool IsAPIDown { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
