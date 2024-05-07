using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class OTPRepositry : BaseRepository<OTPEntity, long>, IOTPRepositry
    {
        public OTPRepositry(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
