using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class ProductWarrantiesConfiguration: BaseEntityTypeConfiguration<ProductWarrantiesEntity,long>
    {
        public override void Config(EntityTypeBuilder<ProductWarrantiesEntity> builder)
        {
            builder.ToTable("ProductWarranties");
        }
    }
}
