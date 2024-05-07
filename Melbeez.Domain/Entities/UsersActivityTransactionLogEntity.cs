using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Melbeez.Domain.Entities
{
    public class UsersActivityTransactionLogEntity : AuditableEntity<long>
    {
        [MaxLength(15)]
        public string IPAddress { get; set; }

        [ForeignKey("CreatedBy")]
        public ApplicationUser applicationUser { get; set; } = null!;
    }
}
