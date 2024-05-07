using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
	public class EmailTransactionLogEntity : AuditableEntity<long> 
	{
		[Required]
		[MaxLength(100)]
		public string To { get; set; } = null!;
		[Required]
		[MaxLength(256)]
		public string Subject { get; set; } = null!;
		[Required]
		public string Body { get; set; } = null!;
		[Required]
		public bool IsAttachments { get; set; }
		[Required]
		public bool IsSuccess { get; set; }
		[Required]
		public int StatusCode { get; set; }
		public string Status { get; set; }
		public string ErrorBody { get; set; }
	}
}