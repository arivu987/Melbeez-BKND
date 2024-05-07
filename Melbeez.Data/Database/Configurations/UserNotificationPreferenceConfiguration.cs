using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
	public class UserNotificationPreferenceConfiguration : BaseEntityTypeConfiguration<UserNotificationPreferenceEntity, long>
	{
		public override void Config(EntityTypeBuilder<UserNotificationPreferenceEntity> builder)
		{
			builder.ToTable("UserNotificationPreferences");
		}
	}
}