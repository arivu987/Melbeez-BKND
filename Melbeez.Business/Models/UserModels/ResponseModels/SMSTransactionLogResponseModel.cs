using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class SMSTransactionLogResponseModel
    {
        public string To { get; set; }
        public string Body { get; set; }
        public bool IsSuccess { get; set; }
        public string SId { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}