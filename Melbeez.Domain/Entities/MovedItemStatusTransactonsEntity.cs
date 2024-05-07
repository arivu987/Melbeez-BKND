using Melbeez.Common.Helpers;
using Melbeez.Domain.Common.BaseEntity;
using System;

namespace Melbeez.Domain.Entities
{
    public class MovedItemStatusTransactonsEntity: AuditableEntity<long>
    {
        public string TransferId { get; set; }
        public long ItemId { get; set; }
        public bool IsProduct { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public MovedStatus Status { get; set; }
        public DateTime? ExpireOn { get; set; }
        public string? DependentProductIds { get; set; }
    }
}
