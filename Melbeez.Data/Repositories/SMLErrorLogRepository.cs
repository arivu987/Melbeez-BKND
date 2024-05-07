using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
	public class SMLErrorLogRepository : BaseRepository<SMLErrorLogEntity, long>, ISMLErrorLogRepository
	{
		public SMLErrorLogRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}