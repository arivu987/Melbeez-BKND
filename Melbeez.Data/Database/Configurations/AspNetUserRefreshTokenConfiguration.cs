using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Database.Configurations
{
    public class AspNetUserRefreshTokenConfiguration : IEntityTypeConfiguration<AspNetUserRefreshTokenEntity>
    {
        public void Configure(EntityTypeBuilder<AspNetUserRefreshTokenEntity> builder)
        {
            builder.ToTable("AspNetUserRefreshTokens");
        }
    }
}
