using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class AddressesConfiguration : BaseEntityTypeConfiguration<AddressesEntity, long>
    {
        public override void Config(EntityTypeBuilder<AddressesEntity> builder)
        {
            builder.ToTable("Addresses");
        }
    }
}
