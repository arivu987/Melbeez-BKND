using System;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ReceiptRequestModel
    {
        public long ReceiptId { get; set; }
        public string ReceiptName { get; set; }
        public string ReceiptImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}
