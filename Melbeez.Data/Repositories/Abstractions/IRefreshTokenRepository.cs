using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories.Abstractions
{
    public interface IRefreshTokenRepository : IBaseRepository<AspNetUserRefreshTokenEntity, long>
    {
        bool IsValid(string refreshToken, string userId);
    }
}
