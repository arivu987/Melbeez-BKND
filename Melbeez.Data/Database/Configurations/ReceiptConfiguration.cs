using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
	public class ReceiptConfiguration : BaseEntityTypeConfiguration<ReceiptEntity, long>
	{
		public override void Config(EntityTypeBuilder<ReceiptEntity> builder)
		{
			builder.ToTable("Receipts");
		}
	}
}