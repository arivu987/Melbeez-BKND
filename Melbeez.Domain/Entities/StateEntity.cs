using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class StateEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;
        public long CountryId { get; set; }

        [ForeignKey("CountryId")]
        public CountryEntity CountryDetails { get; set; } = null!;
    }
}
