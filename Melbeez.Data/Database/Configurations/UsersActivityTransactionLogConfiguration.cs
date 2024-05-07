using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class UsersActivityTransactionLogConfiguration : BaseEntityTypeConfiguration<UsersActivityTransactionLogEntity, long>
    {
        public override void Config(EntityTypeBuilder<UsersActivityTransactionLogEntity> builder)
        {
            builder.ToTable("UsersActivityTransactionLog");
        }
    }
}
