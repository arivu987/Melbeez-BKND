using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
	public class ProductImageRepository : BaseRepository<ProductImageEntity, long>, IProductImageRepository
	{
		public ProductImageRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}