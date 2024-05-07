using System;
using System.ComponentModel.DataAnnotations;
using Melbeez.Domain.Common.BaseEntity;

namespace Melbeez.Domain.Entities
{
    public class AspNetUserRefreshTokenEntity : AuditableEntity<Int64>
    {
        [Required]
        [MaxLength(450)]
        public string UserId { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [MaxLength(256)]
        public string IPAddress { get; set; }
        [Required]
        public DateTime ExpiresOn { get; set; }
    }
}
