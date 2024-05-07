using Melbeez.Common.Models.Entities;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories.Abstractions
{
    public interface ILocationsRepository : IBaseRepository<LocationsEntity, long>
    {
    }
}
