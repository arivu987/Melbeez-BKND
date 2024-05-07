using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
    public class StateConfiguration : BaseEntityTypeConfiguration<StateEntity, long>
    {
        public override void Config(EntityTypeBuilder<StateEntity> builder)
        {
            builder.ToTable("State");
        }
    }
}
