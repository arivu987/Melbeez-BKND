using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class ContactUsConfiguration :BaseEntityTypeConfiguration<ContactUsEntity, long>
    {
        public override void Config(EntityTypeBuilder<ContactUsEntity> builder)
        {
            builder.ToTable("ContactUs");
        }
    }
}
