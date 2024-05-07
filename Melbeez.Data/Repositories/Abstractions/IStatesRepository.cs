using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories.Abstractions
{
    public interface IStatesRepository : IBaseRepository<StateEntity, long>
    {
    }
}
