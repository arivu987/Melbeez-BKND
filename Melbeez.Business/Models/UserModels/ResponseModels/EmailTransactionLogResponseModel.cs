using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class EmailTransactionLogResponseModel
    {
        public string To { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsAttachments { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string ErrorBody { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}