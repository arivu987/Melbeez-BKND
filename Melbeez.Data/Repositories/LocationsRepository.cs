using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class LocationsRepository : BaseRepository<LocationsEntity, long>, ILocationsRepository
    {
        public LocationsRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
