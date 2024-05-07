using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
	public class ReceiptRepository : BaseRepository<ReceiptEntity, long>, IReceiptRepository
	{
		public ReceiptRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}