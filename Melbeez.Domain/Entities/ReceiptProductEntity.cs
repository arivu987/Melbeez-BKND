using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
	public class ReceiptProductEntity : AuditableEntity<long> 
	{
		[Required]
		public long ReceiptId { get; set; }
		[Required]
		public long ProductId { get; set; }
		[ForeignKey("ReceiptId")]
		public ReceiptEntity receiptDetail { get; set; } = null!;
		[ForeignKey("ProductId")]
		public ProductEntity productDetail { get; set; } = null!;
	}
}