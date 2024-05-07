using Melbeez.Domain.Common.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
	public class ReceiptEntity : AuditableEntity<long> 
	{
		[Required]
        [MaxLength(256)]
		public string Name { get; set; } = null!;
		[MaxLength(256)]
		public string? ImageUrl { get; set; }
		public DateTime? PurchaseDate { get; set; }
		public virtual List<ReceiptProductEntity> ReceiptProductDetails { get; set; }
	}
}