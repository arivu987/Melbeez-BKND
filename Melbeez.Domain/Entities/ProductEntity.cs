using Melbeez.Common.Helpers;
using Melbeez.Domain.Common.BaseEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class ProductEntity : AuditableEntity<long>
    {
        [MaxLength(500)]
        public string? Name { get; set; }
        [MaxLength(500)]
        public string? Title { get; set; }
        [Required]
        public long LocationId { get; set; }
        public long? CategoryId { get; set; }
        public string ModelNumber { get; set; }
        public string ManufactureName { get; set; }
        public string SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string VendorName { get; set; }
        [MaxLength(100)]
        public string Currency { get; set; }
        public double Price { get; set; }
        public string Notes { get; set; }
        public string FormData { get; set; }
        public string BarCodeNumber { get; set; }
        public string BarCodeData { get; set; }
        public string OtherInfo { get; set; }
        public bool IsMoving { get; set; }
        public MovedStatus Status { get; set; }
        public long? ProductModelInfoId { get; set; }
        public string TransferTo { get; set; }
        [ForeignKey("LocationId")]
        public LocationsEntity LocationsDetail { get; set; } = null!;
        [ForeignKey("CategoryId")]
        public ProductCategoriesEntity ProductCategoriesDetail { get; set; }
        public virtual List<ProductWarrantiesEntity> ProductWarrantiesDetails { get; set; }
        public virtual List<ProductImageEntity> ProductImageDetails { get; set; }
        public virtual List<ReceiptProductEntity> ReceiptProductDetails { get; set; }
    }
}
