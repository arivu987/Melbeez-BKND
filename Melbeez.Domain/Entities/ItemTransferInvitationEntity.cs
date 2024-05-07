using Melbeez.Domain.Common.BaseEntity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
    public class ItemTransferInvitationEntity : AuditableEntity<long>
    {
        public string CountryCode { get; set; }
        [MaxLength(10)]
        public string PhoneNumber { get; set; }
        [MaxLength(100)]
        public string Email { get; set; }
        public string ItemId { get; set; }
        public bool IsProduct { get; set; }
        public string TransferId { get; set; }
        public bool IsAccepted { get; set; } 
        public DateTime ExpiredOn { get; set; }
    }
}
