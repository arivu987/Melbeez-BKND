using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
    public class APIDownStatusEntity : AuditableEntity<long>
    {
        [Required]
        public bool IsApiDown { get; set; }
    }
}
