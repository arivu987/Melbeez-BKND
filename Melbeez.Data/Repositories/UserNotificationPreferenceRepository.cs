using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
	public class UserNotificationPreferenceRepository : BaseRepository<UserNotificationPreferenceEntity, long>, IUserNotificationPreferenceRepository
	{
		public UserNotificationPreferenceRepository(ApplicationDbContext dbContext) : base(dbContext)
		{
		}
	}
}