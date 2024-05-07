using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class ProductsRepository : BaseRepository<ProductEntity, long>, IProductsRepository
    {
        public ProductsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
