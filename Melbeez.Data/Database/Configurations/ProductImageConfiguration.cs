using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
	public class ProductImageConfiguration : BaseEntityTypeConfiguration<ProductImageEntity, long>
	{
		public override void Config(EntityTypeBuilder<ProductImageEntity> builder)
		{
			builder.ToTable("ProductImages");
		}
	}
}