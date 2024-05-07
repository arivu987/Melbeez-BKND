using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class BarCodeTransactionLogsRepository : BaseRepository<BarCodeTransactionLogsEntity, long>, IBarCodeTransactionLogsRepository
    {
        public BarCodeTransactionLogsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
