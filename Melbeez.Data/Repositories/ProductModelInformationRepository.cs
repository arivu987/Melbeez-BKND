using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class ProductModelInformationRepository : BaseRepository<ProductModelInformationEntity, long>, IProductModelInformationRepository
    {
        public ProductModelInformationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
