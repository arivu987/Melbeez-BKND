using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models;
using Melbeez.Business.Models.Common;
using Melbeez.Data.UnitOfWork;
using Melbeez.Common.Models.Entities;
using Melbeez.Domain.Entities;
using Melbeez.Common.Models;
using System.Linq;
using Melbeez.Common.Extensions;

namespace Melbeez.Business.Managers
{
    public class CountryManager : ICountryManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public CountryManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<CountryViewModel>>> Get(PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .CountryRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new CountryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    CountryCode = x.CountryCode,
                    CurrencyCode = x.CurrencyCode
                })
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.Name.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);

            return new ManagerBaseResponse<IEnumerable<CountryViewModel>>()
            {
                Result = result.Data,
                PageDetail = new PageDetailModel()
                {
                    Skip = pagedListCriteria.Skip,
                    Take = pagedListCriteria.Take,
                    Count = result.TotalCount,
                    SearchText = pagedListCriteria.SearchText
                }
            };
        }
        public async Task<ManagerBaseResponse<CountryViewModel>> Get(long id)
        {
            var result = await unitOfWork
                .CountryRepository
                .GetQueryable(x => !x.IsDeleted && x.Id == id)
                .Select(x => new CountryViewModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    CountryCode = x.CountryCode,
                    CurrencyCode = x.CurrencyCode
                })
                .FirstOrDefaultAsync();
            return new ManagerBaseResponse<CountryViewModel>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddCountry(CountriesRequestModel model, string userId)
        {
            try
            {
                if (model.Id == 0)
                {
                    await unitOfWork.CountryRepository.AddAsync(new CountryEntity()
                    {
                        Name = model.Name,
                        CountryCode = model.CountryCode,
                        CurrencyCode = model.CurrencyCode,
                        IsDeleted = false,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                    });
                    await unitOfWork.CommitAsync();
                }
                else
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Invalid country request.",
                        Result = false
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Country added successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateCountry(CountriesRequestModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                            .CountryRepository
                            .GetAsync(entity => entity.Id == model.Id
                                        && !entity.IsDeleted
                                     );
                if (entity != null)
                {
                    entity.Name = model.Name;
                    entity.CountryCode = model.CountryCode;
                    entity.CurrencyCode = model.CurrencyCode;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
                else
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Invalid country request.",
                        Result = false
                    };

                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Country updated successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> DeleteCountry(long Id, string userId)
        {
            try
            {
                var entity = await unitOfWork
                                .CountryRepository
                                .GetAsync(entity => entity.Id == Id
                                            && !entity.IsDeleted);
                if (entity != null)
                {
                    entity.IsDeleted = true;
                    entity.DeletedBy = userId;
                    entity.DeletedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Country deleted successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false
                };
            }
        }
    }
}
