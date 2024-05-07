using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class SMLErrorLogResponseModel
    {
        public string TrackId { get; set; }
        public string Email { get; set; }
        public bool IsWeb { get; set; }
        public int? ErrorLineNo { get; set; }
        public string ExceptionMsg { get; set; }
        public string ExceptionType { get; set; }
        public string ExceptionDetail { get; set; }
        public string Path { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}