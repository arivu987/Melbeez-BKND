using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
	public class SMSTransactionLogConfiguration : BaseEntityTypeConfiguration<SMSTransactionLogEntity, long>
	{
		public override void Config(EntityTypeBuilder<SMSTransactionLogEntity> builder)
		{
			builder.ToTable("SMSTransactionLogs");
		}
	}
}