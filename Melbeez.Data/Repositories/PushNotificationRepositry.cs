using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class PushNotificationRepositry : BaseRepository<PushNotificationEntity, long>, IPushNotificationRepositry
    {
        public PushNotificationRepositry(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
