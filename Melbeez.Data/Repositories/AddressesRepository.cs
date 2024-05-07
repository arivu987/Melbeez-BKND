using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class AddressesRepository : BaseRepository<AddressesEntity, long>, IAddressesRepository
    {
        public AddressesRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
