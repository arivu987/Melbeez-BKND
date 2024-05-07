using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class APIActivitiesConfiguration : BaseEntityTypeConfiguration<APIActivitiesEntity, long>
    {
        public override void Config(EntityTypeBuilder<APIActivitiesEntity> builder)
        {
            builder.ToTable("APIActivities");
        }
    }
}
