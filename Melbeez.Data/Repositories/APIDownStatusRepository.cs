using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class APIDownStatusRepository : BaseRepository<APIDownStatusEntity, long>, IAPIDownStatusRepository
    {
        public APIDownStatusRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
