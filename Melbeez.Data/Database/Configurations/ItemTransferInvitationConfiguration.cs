using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Data.Database.Configurations
{
    public class ItemTransferInvitationConfiguration : BaseEntityTypeConfiguration<ItemTransferInvitationEntity, long>
    {
        public override void Config(EntityTypeBuilder<ItemTransferInvitationEntity> builder)
        {
            builder.ToTable("ItemTransferInvitations");
        }
    }
}
