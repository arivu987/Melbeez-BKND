using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class RegisterDeviceEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }

        [Required]
        [MaxLength(450)]
        public string DeviceToken { get; set; }

        [Required]
        public string UId { get; set; }

        [Required]
        public int DeviceType { get; set; }

        [Required]
        [Display(Name = "App Version")]
        public string AppVersion { get; set; }

        [Required]
        [Display(Name = "OS Version")]
        public string OSVersion { get; set; }

        [Display(Name = "Latitude")]
        public float Lat { get; set; }

        [Display(Name = "Latitude")]
        public float Long { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser applicationUser { get; set; } = null!;

    }
}
