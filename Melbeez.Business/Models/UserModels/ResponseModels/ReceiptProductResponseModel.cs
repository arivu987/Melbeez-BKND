using System.ComponentModel.DataAnnotations;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ReceiptProductResponseModel
    {
        [Required]
        public long ReceiptId { get; set; }
        [Required]
        public long ProductId { get; set; }
    }
}