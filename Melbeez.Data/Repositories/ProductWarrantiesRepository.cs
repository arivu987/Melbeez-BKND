using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class ProductWarrantiesRepository : BaseRepository<ProductWarrantiesEntity, long>, IProductWarrantiesRepository
    {
        public ProductWarrantiesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
