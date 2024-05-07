using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class APIDownStatusConfiguration : BaseEntityTypeConfiguration<APIDownStatusEntity, long>
    {
        public override void Config(EntityTypeBuilder<APIDownStatusEntity> builder)
        {
            builder.ToTable("APIDownStatus");
        }
    }
}
