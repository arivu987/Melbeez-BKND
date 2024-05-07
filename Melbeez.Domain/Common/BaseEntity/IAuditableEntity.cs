using System;

namespace Melbeez.Domain.Common.BaseEntity
{
    public interface IAuditableEntity<TKey>
    {
        TKey Id { get; set; }
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTime? UpdatedOn { get; set; }
        string UpdatedBy { get; set; }
        DateTime? DeletedOn { get; set; }
        string DeletedBy { get; set; }
        bool IsDeleted { get; set; }
    }
}
