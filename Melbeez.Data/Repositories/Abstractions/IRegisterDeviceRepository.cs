using Melbeez.Common.Models.Entities;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories.Abstractions
{
    public interface IRegisterDeviceRepository : IBaseRepository<RegisterDeviceEntity, long>
    {
    }
}
