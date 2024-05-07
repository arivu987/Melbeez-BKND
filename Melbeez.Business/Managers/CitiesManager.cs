using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class CitiesManager : ICitiesManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public CitiesManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<CitiesResponseModel>>> Get(PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .CitiesRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new CitiesResponseModel()
                {
                    Id = x.Id,
                    CityName = x.Name,
                    StateId = x.StateId,
                    StateName = x.StateDetails.Name,
                    CountryId = x.StateDetails.CountryId,
                    CountryName = x.StateDetails.CountryDetails.Name
                })
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.StateName.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);

            return new ManagerBaseResponse<IEnumerable<CitiesResponseModel>>()
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
        public async Task<ManagerBaseResponse<IEnumerable<CitiesResponseModel>>> GetCityByStateId(long stateId)
        {
            var result = await unitOfWork
                .CitiesRepository
                .GetQueryable(x => !x.IsDeleted && x.StateId == stateId)
                .Select(x => new CitiesResponseModel()
                {
                    Id = x.Id,
                    CityName = x.Name,
                    StateId = x.StateId
                })
                .OrderBy(x => x.CityName)
                .AsNoTracking()
                .ToListAsync();

            return new ManagerBaseResponse<IEnumerable<CitiesResponseModel>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<CitiesResponseModel>> GetCityById(long id)
        {
            var result = await unitOfWork
                .CitiesRepository
                .GetQueryable(x => !x.IsDeleted && x.Id == id)
                .Select(x => new CitiesResponseModel()
                {
                    Id = x.Id,
                    CityName = x.Name,
                    StateId = x.StateId
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
            return new ManagerBaseResponse<CitiesResponseModel>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<CitiesResponseModel>> AddCity(CitiesRequestModel model, string userId)
        {
            try
            {
                if (model.Id == 0)
                {

                    var cities = await unitOfWork
                                .CitiesRepository
                                .GetQueryable()
                                .ToListAsync();

                    if (cities.Any())
                    {
                        if (cities.Where(x => x.StateId == model.StateId).Any(x => x.Name.ToLower() == model.CityName.ToLower()))
                        {
                            return new ManagerBaseResponse<CitiesResponseModel>()
                            {
                                Message = "City already exists. (City : " + model.CityName + ")",
                                Result = null,
                                StatusCode = StatusCodes.Status409Conflict
                            };
                        }
                        var lastId  = cities.OrderByDescending(x => x.Id).FirstOrDefault().Id;
                        model.Id = lastId + 1;
                    }

                    var response = await unitOfWork.CitiesRepository.AddAsync(new CitiesEntity()
                    {
                        Id = model.Id,
                        StateId = model.StateId,
                        Name = model.CityName,
                        IsDeleted = false,
                        CreatedBy = userId,
                        CreatedOn = DateTime.UtcNow,
                    });
                    await unitOfWork.CommitAsync();

                    return new ManagerBaseResponse<CitiesResponseModel>()
                    {
                        Message = "City added successfully.",
                        Result = new CitiesResponseModel()
                        {
                            Id = response.Id,
                            CityName = response.Name,
                            StateId = response.StateId
                        }
                    };
                }
                return new ManagerBaseResponse<CitiesResponseModel>()
                {
                    Message = "Invalid city request.",
                    Result = null,
                    StatusCode = 500
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<CitiesResponseModel>()
                {
                    Message = ex.Message,
                    Result = null,
                    StatusCode = 500
                };
            }
        }
        public async Task<ManagerBaseResponse<bool>> UpdateCity(CitiesRequestModel model, string userId)
        {
            try
            {
                var entity = await unitOfWork
                            .CitiesRepository
                            .GetAsync(entity => entity.Id == model.Id
                                        && !entity.IsDeleted
                                     );
                if (entity != null)
                {
                    entity.StateId = model.StateId;
                    entity.Name = model.CityName;
                    entity.UpdatedBy = userId;
                    entity.UpdatedOn = DateTime.UtcNow;
                    await unitOfWork.CommitAsync();
                }
                else
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "Invalid city request.",
                        Result = false,
                        StatusCode = 500
                    };

                }
                return new ManagerBaseResponse<bool>()
                {
                    Message = "City updated successfully.",
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
        public async Task<ManagerBaseResponse<bool>> DeleteCity(long Id, string userId)
        {
            try
            {
                var entity = await unitOfWork
                                .CitiesRepository
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
                    Message = "City deleted successfully.",
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
