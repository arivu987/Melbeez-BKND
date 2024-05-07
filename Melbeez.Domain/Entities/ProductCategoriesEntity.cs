using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
    public class ProductCategoriesEntity: AuditableEntity<long>
    {
        [Required]
        [MaxLength(500)]
        public string Name { get; set; }
        [Required]
        public string FormBuilderData { get; set; }
        [Required]
        public int Sequence { get; set; }
        [Required]
        public bool IsActive { get; set; }
    }
}
