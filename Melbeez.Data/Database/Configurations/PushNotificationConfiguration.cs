using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class PushNotificationConfiguration : BaseEntityTypeConfiguration<PushNotificationEntity, long>
    {
        public override void Config(EntityTypeBuilder<PushNotificationEntity> builder)
        {
            builder.ToTable("PushNotifications");
        }
    }
}
