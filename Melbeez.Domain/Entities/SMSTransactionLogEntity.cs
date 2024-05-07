using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
	public class SMSTransactionLogEntity : AuditableEntity<long> 
	{
		[Required]
		[MaxLength(20)]
		public string To { get; set; } = null!;
		[Required]
		public string Body { get; set; } = null!;
		[Required]
		public bool IsSuccess { get; set; }
		[MaxLength(50)]
		public string SId { get; set; }
		[Required]
		public int StatusCode { get; set; }
		public string Status { get; set; }
		public string ErrorMessage { get; set; }
	}
}