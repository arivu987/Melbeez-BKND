using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class APIActivitiesRepository : BaseRepository<APIActivitiesEntity, long>, IAPIActivitiesRepository
    {
        public APIActivitiesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
