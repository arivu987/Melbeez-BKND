using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class RegisterDeviceConfiguration : BaseEntityTypeConfiguration<RegisterDeviceEntity, long>
    {
        public override void Config(EntityTypeBuilder<RegisterDeviceEntity> builder)
        {
            builder.ToTable("RegisterDevice");
        }
    }
}
