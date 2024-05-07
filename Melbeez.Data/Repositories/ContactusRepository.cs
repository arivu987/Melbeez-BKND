using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class ContactusRepository : BaseRepository<ContactUsEntity, long>, IContactusRepository
    {
        public ContactusRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
