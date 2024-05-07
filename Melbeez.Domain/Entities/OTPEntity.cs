using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class OTPEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
        [Required]
        [MaxLength(10)]
        public string OTP { get; set; }
        [MaxLength(10)]
        public string PhoneNumber { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        [Required]
        public bool IsUsed { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser applicationUser { get; set; } = null!;
    }
}
