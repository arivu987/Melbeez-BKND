using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class EmailTransactionLogManager : IEmailTransactionLogManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public EmailTransactionLogManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<EmailTransactionLogResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .EmailTransactionLogRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new EmailTransactionLogResponseModel()
                {
                    To = x.To,
                    Subject = x.Subject,
                    Body = x.Body.Replace("\r\n ", "").Trim(),
                    IsAttachments = x.IsAttachments,
                    IsSuccess = x.IsSuccess,
                    StatusCode = x.StatusCode,
                    Status = x.Status,
                    ErrorBody = x.ErrorBody,
                    CreatedOn = x.CreatedOn
                })
                .WhereIf(startDate.HasValue && !(endDate.HasValue), w => w.CreatedOn >= startDate.Value.ToUniversalTime())
                .WhereIf(endDate.HasValue && !(startDate.HasValue), w => w.CreatedOn.Date <= endDate.Value.ToUniversalTime())
                .WhereIf(startDate.HasValue && endDate.HasValue, w => w.CreatedOn >= startDate.Value.ToUniversalTime() && w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.To.Contains(pagedListCriteria.SearchText)
                                                                                    || x.Subject.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                    || x.Status.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<EmailTransactionLogResponseModel>>()
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
        public async Task<ManagerBaseResponse<bool>> AddEmailTransactionLog(EmailTransactionLogResponseModel model, string userId)
        {
            try
            {
                await unitOfWork.EmailTransactionLogRepository.AddAsync(new EmailTransactionLogEntity()
                {
                    To = model.To,
                    Subject = model.Subject,
                    Body = JsonConvert.SerializeObject(model.Body),
                    IsAttachments = model.IsAttachments,
                    IsSuccess = model.IsSuccess,
                    StatusCode = model.StatusCode,
                    Status = model.Status,
                    ErrorBody = model.ErrorBody,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                });
                await unitOfWork.CommitAsync();
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Email Transaction Log added successfully.",
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