using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class ItemTransferRepository : BaseRepository<MovedItemStatusTransactonsEntity, long>, IItemTransferRepository
    {
        public ItemTransferRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
