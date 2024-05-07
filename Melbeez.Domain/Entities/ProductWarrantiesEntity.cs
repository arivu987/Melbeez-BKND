using Melbeez.Domain.Common.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class ProductWarrantiesEntity : AuditableEntity<long>
    {
        [Required]
        public long ProductId { get; set; }
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [MaxLength(10)]
        public string Currency { get; set; }
        public double? Price { get; set; }
        [MaxLength(256)]
        public string Provider { get; set; }
        [MaxLength(256)]
        public string Type { get; set; }
        [MaxLength(256)]
        public string AgreementNumber { get; set; }
        [MaxLength(256)]
        public string ImageUrl { get; set; }
        public bool IsProduct { get; set; }
        [ForeignKey("ProductId")]
        public ProductEntity ProductDetail { get; set; } = null!;
    }
}