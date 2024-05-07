using Melbeez.Domain.Common.BaseEntity;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Data.Database.Configurations
{
    public class ProductModelInformationConfiguration : BaseEntityTypeConfiguration<ProductModelInformationEntity, long>
    {
        public override void Config(EntityTypeBuilder<ProductModelInformationEntity> builder)
        {
            builder.ToTable("ProductModelInformation");
        }
    }
}
