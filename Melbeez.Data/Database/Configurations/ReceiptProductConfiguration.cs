using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
	public class ReceiptProductConfiguration : BaseEntityTypeConfiguration<ReceiptProductEntity, long>
	{
		public override void Config(EntityTypeBuilder<ReceiptProductEntity> builder)
		{
			builder.ToTable("ReceiptProducts");
		}
	}
}