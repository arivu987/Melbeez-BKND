using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class ProductCategoriesRepository: BaseRepository<ProductCategoriesEntity, long>, IProductCategoriesRepository
    {
        public ProductCategoriesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
