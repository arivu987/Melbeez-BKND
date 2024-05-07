using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class ContactUsEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = null!;
        [Required]
        [MaxLength(256)]
        public string Message { get; set; } = null!;
        [MaxLength(256)]
        public string Image { get; set; }
        [ForeignKey("CreatedBy")]
        public ApplicationUser applicationUser { get; set; } = null!;
    }
}
