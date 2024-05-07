using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;
using System;
using System.Linq;

namespace Melbeez.Data.Repositories
{
    public class RefreshTokenRepository : BaseRepository<AspNetUserRefreshTokenEntity, long>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public bool IsValid(string refreshToken, string userId)
        {
            return GetQueryable()
                .Any(x => x.UserId == userId && x.Token == refreshToken && DateTime.UtcNow <= x.ExpiresOn);
        }
    }
}
