using Melbeez.Domain.Common.BaseEntity;
using System;

namespace Melbeez.Domain.Entities
{
    public class APIActivitiesEntity : AuditableEntity<long>
    {
        public string APIPath { get; set; }
        public int ExecutionTime { get; set; }
    }
}
