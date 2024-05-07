using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ReceiptResponseModel
    {
        public long ReceiptId { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string ImageUrl { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int ProductAsscociatedCount { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class ReceiptDetailResponseModel : ReceiptResponseModel
    {
        public List<ProductsResponseModel> ProductDetails { get; set; }
    }
}