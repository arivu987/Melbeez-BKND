using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
	public class ProductImageEntity : AuditableEntity<long> 
	{
		[Required]
		public long ProductId { get; set; }
		[Required]
		public string ImageUrl { get; set; } = null!;
		[Required]
		public int FileSize { get; set; }
		[Required]
		public bool IsDefault { get; set; }
		[ForeignKey("ProductId")]
		public ProductEntity ProductDetail { get; set; } = null!;
	}
}