using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
	public class SMLErrorLogEntity : AuditableEntity<long> 
	{
		[Required]
		[MaxLength(50)]
		public string TrackId { get; set; } = null!;
        [Required]
		public string Email { get;set; }
		[Required]
		public bool IsWeb { get; set; }
		public int? ErrorLineNo { get; set; }
		public string ExceptionMsg { get; set; }
		public string ExceptionType { get; set; }
		public string ExceptionDetail { get; set; }
		[MaxLength(100)]
		public string Path { get; set; }
	}
}