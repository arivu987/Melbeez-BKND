using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class ItemTransferInvitationRepository : BaseRepository<ItemTransferInvitationEntity, long>, IItemTransferInvitationRepository
    {
        public ItemTransferInvitationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
