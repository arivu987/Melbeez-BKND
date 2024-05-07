using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
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
    public class SMLErrorLogManager : ISMLErrorLogManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public SMLErrorLogManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<SMLErrorLogResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .SMLErrorLogRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new SMLErrorLogResponseModel()
                {
                    TrackId = x.TrackId,
                    Email = x.Email,
                    IsWeb = x.IsWeb,
                    ErrorLineNo = x.ErrorLineNo,
                    ExceptionMsg = x.ExceptionMsg,
                    ExceptionType = x.ExceptionType,
                    ExceptionDetail = x.ExceptionDetail,
                    Path = x.Path,
                    CreatedOn = x.CreatedOn
                })
                .WhereIf(startDate.HasValue && !(endDate.HasValue), w => w.CreatedOn >= startDate.Value.ToUniversalTime())
                .WhereIf(endDate.HasValue && !(startDate.HasValue), w => w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(startDate.HasValue && endDate.HasValue, w => w.CreatedOn >= startDate.Value.ToUniversalTime() && w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.ExceptionMsg.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                    || x.ExceptionType.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<SMLErrorLogResponseModel>>()
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
        public async Task<ManagerBaseResponse<string>> Add(SMLErrorLogResponseModel model, string userId)
        {
            try
            {
                var response = await unitOfWork.SMLErrorLogRepository.AddAsync(new SMLErrorLogEntity()
                {
                    TrackId = UtilityHelper.GetUniqueId(),
                    Email = model.Email,
                    IsWeb = model.IsWeb,
                    ErrorLineNo = model.ErrorLineNo,
                    ExceptionMsg = model.ExceptionMsg,
                    ExceptionType = model.ExceptionType,
                    ExceptionDetail = model.ExceptionDetail,
                    Path = model.Path,
                    IsDeleted = false,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                });
                await unitOfWork.CommitAsync();
                return new ManagerBaseResponse<string>()
                {
                    Message = "SML Error Log added successfully.",
                    Result = response.TrackId
                };
            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<string>()
                {
                    Message = ex.Message,
                    Result = null,
                    StatusCode = 500
                };
            }
        }
    }
}