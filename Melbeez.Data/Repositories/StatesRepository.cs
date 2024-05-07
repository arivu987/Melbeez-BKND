using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class StatesRepository : BaseRepository<StateEntity, long>, IStatesRepository
    {
        public StatesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
