using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
    public class CountryConfiguration : BaseEntityTypeConfiguration<CountryEntity, long>
    {
        public override void Config(EntityTypeBuilder<CountryEntity> builder)
        {
            builder.ToTable("Country");
        }
    }
}
