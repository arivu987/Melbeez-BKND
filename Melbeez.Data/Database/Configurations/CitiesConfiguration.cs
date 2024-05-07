using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class CitiesConfiguration : BaseEntityTypeConfiguration<CitiesEntity, long>
    {
        public override void Config(EntityTypeBuilder<CitiesEntity> builder)
        {
            builder.ToTable("Cities");
        }
    }
}
