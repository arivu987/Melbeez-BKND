using System;

namespace Melbeez.Business.Models
{
    public class BarCodeTransactionLogsResponseModel
    {
        public long Id { get; set; }
        public string BarCode { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set;}

    }
    public class BarCodeTransactionLogsRequestModel
    {
        public string BarCode { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
