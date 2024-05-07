using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class AdminTransactionLogConfiguration : BaseEntityTypeConfiguration<AdminTransactionLogEntity, long>
	{
		public override void Config(EntityTypeBuilder<AdminTransactionLogEntity> builder)
		{
			builder.ToTable("AdminTransactionLogs");
		}
	}
}
