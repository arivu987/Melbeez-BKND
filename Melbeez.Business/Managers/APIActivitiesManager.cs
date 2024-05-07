using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models;
using Melbeez.Business.Models.Common;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class APIActivitiesManager : IAPIActivitiesManager
    {
        private readonly IUnitOfWork unitOfWork;
        public APIActivitiesManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ManagerBaseResponse<IEnumerable<APIActivitiesResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria)
        {
            var isNumeric = int.TryParse(pagedListCriteria.SearchText, out int n);
            var executionTime = -1;
            if (isNumeric)
            {
                executionTime = Convert.ToInt32(pagedListCriteria.SearchText);
            }
            var result = await unitOfWork
                .APIActivitiesRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new APIActivitiesResponseModel()
                {
                    Id = x.Id,
                    APIPath = x.APIPath,
                    AvarageExecutionTime = x.ExecutionTime,
                    CreatedBy = x.CreatedBy,
                    CreatedOn = x.CreatedOn
                })
                .Where(x => !x.APIPath.Contains("/swagger/"))
                .WhereIf(startDate.HasValue && !(endDate.HasValue), w => w.CreatedOn >= startDate.Value.ToUniversalTime())
                .WhereIf(endDate.HasValue && !(startDate.HasValue), w => w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(startDate.HasValue && endDate.HasValue, w => w.CreatedOn >= startDate.Value.ToUniversalTime() && w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText) || executionTime > -1, x => x.APIPath.ToLower().Contains(pagedListCriteria.SearchText.ToLower()) || x.AvarageExecutionTime.Equals(executionTime))
                .AsNoTracking()
                .ToListAsync();

            var groupByapiActivity = result.GroupBy(x => new { x.APIPath, CreatedOn = x.CreatedOn.ToString("yyyy-MM-dd") }).Select(y => y.First()).OrderByDescending(o => o.CreatedOn).ToList();

            var apiActivityResponseList = result
                .GroupBy(x => new
                {
                    x.APIPath,
                    CreatedOn = x.CreatedOn.ToString("yyyy-MM-dd")
                })
                .Select(x => new APIActivitiesResponseModel()
                {
                    APIPath = x.Key.APIPath,
                    AvarageExecutionTime = x.Average(y => y.AvarageExecutionTime),
                    TotalNumberOfCall = x.Count(),
                    CreatedOn = Convert.ToDateTime(x.Key.CreatedOn).Date
                });

            return new ManagerBaseResponse<IEnumerable<APIActivitiesResponseModel>>()
            {
                Result = apiActivityResponseList.Skip(pagedListCriteria.Skip).Take(pagedListCriteria.Take).ToList(),
                PageDetail = new PageDetailModel()
                {
                    Skip = pagedListCriteria.Skip,
                    Take = pagedListCriteria.Take,
                    Count = groupByapiActivity.Count(),
                    SearchText = pagedListCriteria.SearchText
                }
            };
        }
        public async Task<ManagerBaseResponse<bool>> Add(APIActivitiesRequestModel model, string userId = "-1")
        {
            try
            {
                await unitOfWork.APIActivitiesRepository.AddAsync(new APIActivitiesEntity()
                {
                    APIPath = model.APIPath,
                    ExecutionTime = model.ExecutionTime,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                });
                await unitOfWork.CommitAsync();

                return new ManagerBaseResponse<bool>()
                {
                    Message = "API Activity added successfully.",
                    Result = true
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Message = ex.InnerException != null ? ex.InnerException.Message : ex.Message,
                    Result = false,
                    StatusCode = 500
                };
            }
        }
    }
}
