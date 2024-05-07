using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class CitiesEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;
        public long StateId { get; set; }

        [ForeignKey("StateId")]
        public StateEntity StateDetails { get; set; } = null!;
    }
}
