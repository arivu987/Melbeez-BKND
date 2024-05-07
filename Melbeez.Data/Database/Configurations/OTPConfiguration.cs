using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class OTPConfiguration : BaseEntityTypeConfiguration<OTPEntity, long>
    {
        public override void Config(EntityTypeBuilder<OTPEntity> builder)
        {
            builder.ToTable("OTP");
        }
    }
}
