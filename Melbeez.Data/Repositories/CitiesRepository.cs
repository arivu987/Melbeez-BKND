using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class CitiesRepository : BaseRepository<CitiesEntity, long>, ICitiesRepository
    {
        public CitiesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
