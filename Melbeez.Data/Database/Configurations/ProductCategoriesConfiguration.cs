using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class ProductCategoriesConfiguration :BaseEntityTypeConfiguration<ProductCategoriesEntity, long>
    {
        public override void Config(EntityTypeBuilder<ProductCategoriesEntity> builder)
        {
            builder.ToTable("ProductCategories");
        }
    }
}
