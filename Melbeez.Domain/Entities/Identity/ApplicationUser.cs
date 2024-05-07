using Melbeez.Common.Helpers;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(256)]
        public string FirstName { get; set; }
        [MaxLength(256)]
        public string LastName { get; set; }
        [StringLength(1024)]
        public string ProfileUrl { get; set; }
        [Column(TypeName = "date")]
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
        public bool IsUserBlockedByAdmin { get; set; } = false;
        public bool IsVerifiedByAdmin { get; set; } = false;
        public bool IsPermanentLockOut { get; set; } = false;
        public bool IsFirstLoginAttempt { get; set; } = false;
        public DateTime VerificationRemindedOn { get; set; }
        public int VerificationReminderCount { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get ; set; }
        public bool IsDeleted { get; set; }
        public bool IsUser { get; set; } = true;
    }
}