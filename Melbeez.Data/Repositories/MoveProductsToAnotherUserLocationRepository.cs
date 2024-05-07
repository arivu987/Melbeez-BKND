using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class MoveProductsToAnotherUserLocationRepository : BaseRepository<MovedItemStatusTransactonsEntity, long>, IMoveProductsToAnotherUserLocationRepository
    {
        public MoveProductsToAnotherUserLocationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
