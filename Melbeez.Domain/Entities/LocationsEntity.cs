using Melbeez.Common.Helpers;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class LocationsEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; set; }
        [Required]
        [MaxLength(256)]
        public string AddressLine1 { get; set; }
        [MaxLength(256)]
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public long? CityId { get; set; }
        [Required]
        [MaxLength(100)]
        public string ZipCode { get; set; }
        public long? StateId { get; set; }
        public long? CountryId { get; set; }
        [MaxLength(256)]
        public string Image { get; set; }
        [Required]
        public LocationType TypeOfProperty { get; set; }
        [Required]
        public bool IsDefault { get; set; }
        public bool IsMoving { get; set; }
        public MovedStatus Status { get; set; }
        public string TransferTo { get; set; }
        [ForeignKey("CreatedBy")]
        public ApplicationUser applicationUser { get; set; } = null!;
        public virtual List<ProductEntity> ProductDetail { get; set; }
    }
}
