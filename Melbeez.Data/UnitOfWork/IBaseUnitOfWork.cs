using System;
using System.Threading.Tasks;

namespace Melbeez.Data.UnitOfWork
{
    public interface IBaseUnitOfWork : IDisposable
    {
        Task CommitAsync();
    }
}
