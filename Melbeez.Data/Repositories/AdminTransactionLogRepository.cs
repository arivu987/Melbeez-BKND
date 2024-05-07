using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class AdminTransactionLogRepository : BaseRepository<AdminTransactionLogEntity, long>, IAdminTransactionLogRepository
    {
        public AdminTransactionLogRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
