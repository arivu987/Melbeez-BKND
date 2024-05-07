using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
	public class EmailTransactionLogConfiguration : BaseEntityTypeConfiguration<EmailTransactionLogEntity, long>
	{
		public override void Config(EntityTypeBuilder<EmailTransactionLogEntity> builder)
		{
			builder.ToTable("EmailTransactionLogs");
		}
	}
}