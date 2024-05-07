using Melbeez.Common.Helpers;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class AddressesEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string AddressLine1 { get; set; }
        [MaxLength(256)]
        public string AddressLine2 { get; set; }
        public string? CityName { get; set; }
        public string? StateName { get; set; }
        public string? CountryName { get; set; }
        public string ZipCode { get; set; }
        public long? CityId { get; set; }
        public long? StateId { get; set; }
        public long? CountryId { get; set; }
        [Required]
        public AddressType TypeOfProperty { get; set; }
        [Required]
        public bool IsDefault { get; set; }
        public bool IsSameMailingAddress { get; set; }
        [ForeignKey("CreatedBy")]
        public ApplicationUser applicationUser { get; set; } = null!;
    }
}
