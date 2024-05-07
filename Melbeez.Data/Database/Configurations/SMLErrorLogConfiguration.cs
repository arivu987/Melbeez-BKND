using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
	public class SMLErrorLogConfiguration : BaseEntityTypeConfiguration<SMLErrorLogEntity, long>
	{
		public override void Config(EntityTypeBuilder<SMLErrorLogEntity> builder)
		{
			builder.ToTable("SMLErrorLogs");
		}
	}
}