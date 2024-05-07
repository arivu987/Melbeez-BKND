using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
    public class LocationsConfiguration : BaseEntityTypeConfiguration<LocationsEntity, long>
    {
        public override void Config(EntityTypeBuilder<LocationsEntity> builder)
        {
            builder.ToTable("Locations");
        }
    }
}
