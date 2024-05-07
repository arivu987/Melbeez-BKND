using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class MovedItemStatusTransactonsConfiguration : BaseEntityTypeConfiguration<MovedItemStatusTransactonsEntity, long>
    {
        public override void Config(EntityTypeBuilder<MovedItemStatusTransactonsEntity> builder)
        {
            builder.ToTable("MovedItemStatusTransactons");
        }
    }
}
