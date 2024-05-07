using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Melbeez.Domain.Common.BaseEntity
{
    public abstract class AuditableEntity<TKey> : IAuditableEntity<TKey>
    {
        public TKey Id { get; set; }
        [Required]
        [JsonIgnore]
        public DateTime CreatedOn { get; set; }
        [Required]
        [MaxLength(450)]
        [JsonIgnore]
        public string CreatedBy { get; set; }
        [JsonIgnore]
        public DateTime? UpdatedOn { get; set; }
        [MaxLength(450)]
        [JsonIgnore]
        public string UpdatedBy { get; set; }
        [JsonIgnore]
        public DateTime? DeletedOn { get; set; }
        [MaxLength(450)]
        [JsonIgnore]
        public string DeletedBy { get; set; }
        //[Required]
        [JsonIgnore]
        public bool IsDeleted { get; set; }
    }
}
