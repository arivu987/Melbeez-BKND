using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class StatesManager : IStatesManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public StatesManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<StatesResponseModel>>> Get(PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .StatesRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new StatesResponseModel()
                {
                    Id = x.Id,
                    StateName = x.Name,
                    CountryId = x.CountryId,
                    CountryName = x.CountryDetails.Name
                })
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.StateName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);

            return new ManagerBaseResponse<IEnumerable<StatesResponseModel>>()
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
        public async Task<ManagerBaseResponse<StatesResponseModel>> GetStateById(long id)
        {
            var result = await unitOfWork
                .StatesRepository
                .GetQueryable(x => !x.IsDeleted && x.Id == id)
                .Select(x => new StatesResponseModel()
                {
                    Id = x.Id,
                    StateName = x.Name,
                    CountryId = x.CountryId
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return new ManagerBaseResponse<StatesResponseModel>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<IEnumerable<StatesResponseModel>>> GetStateByCountryId(long countryId)
        {
            var result = await unitOfWork
                .StatesRepository
                .GetQueryable(x => !x.IsDeleted && x.CountryId == countryId)
                .Select(x => new StatesResponseModel()
                {
                    Id = x.Id,
                    StateName = x.Name,
                    CountryId = x.CountryId
                })
                .OrderBy(x => x.StateName)
                .AsNoTracking()
                .ToListAsync();

            return new ManagerBaseResponse<IEnumerable<StatesResponseModel>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddState(StatesRequestModel model, string userId)
        {
            try
            {
                if (model.Id == 0)
                {
                    await unitOfWork.StatesRepository.AddAsync(new StateEntity()
                    {
                        CountryId = model.CountryId,
                        Name = model.StateName,
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
                        Message = "Invalid state request.",
                        Result = false,
                        StatusCode = 500
                    };
                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "State added successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false,
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateState(StatesRequestModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                            .StatesRepository
                            .GetAsync(entity => entity.Id == model.Id
                                        && !entity.IsDeleted
                                     );
                if (entity != null)
                {
                    entity.CountryId = model.CountryId;
                    entity.Name = model.StateName;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
                else
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Invalid state request.",
                        Result = false,
                        StatusCode = 500
                    };

                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "State updated successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false,
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> DeleteState(long Id, string userId)
        {
            try
            {
                var entity = await unitOfWork
                                .StatesRepository
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
                    Message = "State deleted successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.Message,
                    Result = false,
                    StatusCode = 500
                };
            }
        }
    }
}
