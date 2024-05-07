using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class ProductConfiguration : BaseEntityTypeConfiguration<ProductEntity, long>
    {
        public override void Config(EntityTypeBuilder<ProductEntity> builder)
        {
            builder.ToTable("Products");
        }
    }
}
