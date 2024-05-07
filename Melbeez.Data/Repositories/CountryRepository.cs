using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Melbeez.Data.Database;
using Melbeez.Data.Repositories.Abstractions;
using Melbeez.Domain.Common.BaseRepository;
using Melbeez.Domain.Entities;

namespace Melbeez.Data.Repositories
{
    public class CountryRepository : BaseRepository<CountryEntity, long>, ICountryRepository
    {
        public CountryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
