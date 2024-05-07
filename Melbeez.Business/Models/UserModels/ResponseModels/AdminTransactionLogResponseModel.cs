using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class AdminTransactionLogResponseModel
    {
        public long TransactionId { get; set; }
        public string UserId { get; set; }
        public string TransactionDescription { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
