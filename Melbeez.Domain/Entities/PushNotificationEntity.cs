using Melbeez.Common.Helpers;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class PushNotificationEntity : AuditableEntity<long>
    {
        [Required]
        [MaxLength(256)]
        public string RecipientId { get; set; }
        [Required]
        [MaxLength(50)]
        public NotificationType Type { get; set; }
        [Required]
        [MaxLength(256)]
        public string Title { get; set; }
        [Required]
        [MaxLength(256)]
        public string Description { get; set; }
        [Required]
        [MaxLength(20)]
        public bool IsSuccess { get; set; }
        public bool IsRead { get; set; }
        [MaxLength(256)]
        public string ReferenceId { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string ErrorMeassge { get; set; }
        public MovedStatus? Status { get; set; }

        [ForeignKey("RecipientId")]
        public ApplicationUser applicationUser { get; set; } = null!;
    }
}
