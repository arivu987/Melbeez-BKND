using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class UsersActivityTransactionLogRepository : BaseRepository<UsersActivityTransactionLogEntity, long>, IUsersActivityTransactionLogRepository
    {
        public UsersActivityTransactionLogRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
