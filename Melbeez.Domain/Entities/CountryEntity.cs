using System.ComponentModel.DataAnnotations;
using Melbeez.Domain.Common.BaseEntity;

namespace Melbeez.Domain.Entities
{
    public class CountryEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = null!;
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }

    }
}
