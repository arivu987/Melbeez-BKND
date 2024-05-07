using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Melbeez.Domain.Common.BaseEntity
{
    public abstract class BaseEntityTypeConfiguration<T, TKey>
        : IEntityTypeConfiguration<T> where T : AuditableEntity<TKey>
    {
        public abstract void Config(EntityTypeBuilder<T> builder);

        public void Configure(EntityTypeBuilder<T> builder)
        {
            if (builder == null)
            {
                return;
            }

            builder.HasKey(x => x.Id);

            Config(builder);
        }
    }
}
