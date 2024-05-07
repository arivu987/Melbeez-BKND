using System;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Common.Models.Entities
{
    public abstract class AspNetUserRefreshTokenModel
    {
        [Key]
        public Int64 Id { get; set; }
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
