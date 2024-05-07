using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
	public class ReceiptProductRepository : BaseRepository<ReceiptProductEntity, long>, IReceiptProductRepository
	{
		public ReceiptProductRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}