using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories.Abstractions
{
    public interface IBarCodeTransactionLogsRepository : IBaseRepository<BarCodeTransactionLogsEntity, long>
    {
    }
}
