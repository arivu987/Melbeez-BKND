using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
    public class AdminTransactionLogEntity : AuditableEntity<long>
    {
		[MaxLength(256)]
		public string UserId { get; set; }
		[Required]
		[MaxLength(256)]
		public string LogDescription { get; set; } = null!;
		public string OldStatus { get; set; }
		public string NewStatus { get; set; }
	}
}
