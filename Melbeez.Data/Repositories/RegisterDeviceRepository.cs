using Melbeez.Common.Models.Entities;
using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;
using System;

namespace Melbeez.Data.Repositories
{
    public class RegisterDeviceRepository : BaseRepository<RegisterDeviceEntity, long>, IRegisterDeviceRepository
    {
        public RegisterDeviceRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
