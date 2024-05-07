using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
	public class EmailTransactionLogRepository : BaseRepository<EmailTransactionLogEntity, long>, IEmailTransactionLogRepository
	{
		public EmailTransactionLogRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}