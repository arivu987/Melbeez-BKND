using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
	public class SMSTransactionLogRepository : BaseRepository<SMSTransactionLogEntity, long>, ISMSTransactionLogRepository
	{
		public SMSTransactionLogRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}