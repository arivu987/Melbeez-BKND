using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Common.Services.Abstraction;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class SMSTransactionLogManager : ISMSTransactionLogManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        private readonly IEmailSenderService emailSenderService;
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;
        public SMSTransactionLogManager(IUnitOfWork unitOfWork,
                                        IEmailSenderService emailSenderService,
                                        IWebHostEnvironment environment,
                                        IConfiguration configuration)
        {
            this.unitOfWork = unitOfWork;
            this.emailSenderService = emailSenderService;
            this.environment = environment;
            this.configuration = configuration;
        }
        public async Task<ManagerBaseResponse<IEnumerable<SMSTransactionLogResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria)
        {
            if (!string.IsNullOrEmpty(pagedListCriteria.SearchText))
            {
                pagedListCriteria.SearchText = Uri.UnescapeDataString(pagedListCriteria.SearchText);
            }
            var result = await unitOfWork
                .SMSTransactionLogRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new SMSTransactionLogResponseModel()
                {
                    To = x.To,
                    Body = x.Body.Replace("\r\n", "").Replace("\t", "").Trim(),
                    IsSuccess = x.IsSuccess,
                    SId = x.SId,
                    StatusCode = x.StatusCode,
                    Status = x.Status,
                    ErrorMessage = x.ErrorMessage,
                    CreatedOn = x.CreatedOn
                })
                .WhereIf(startDate.HasValue && !(endDate.HasValue), w => w.CreatedOn >= startDate.Value.ToUniversalTime())
                .WhereIf(endDate.HasValue && !(startDate.HasValue), w => w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(startDate.HasValue && endDate.HasValue, w => w.CreatedOn >= startDate.Value.ToUniversalTime() && w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.To.Contains(pagedListCriteria.SearchText)
                                                                                    || x.Body.ToLower().Contains(pagedListCriteria.SearchText.ToLower())
                                                                                    || x.Status.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<SMSTransactionLogResponseModel>>()
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
        public async Task<ManagerBaseResponse<bool>> AddSMSTransactionLog(SMSTransactionLogResponseModel model, string userId)
        {
            try
            {
                await unitOfWork.SMSTransactionLogRepository.AddAsync(new SMSTransactionLogEntity()
                {
                    To = model.To,
                    Body = model.Body,
                    IsSuccess = model.IsSuccess,
                    SId = model.SId,
                    StatusCode = model.StatusCode,
                    Status = model.Status,
                    ErrorMessage = model.ErrorMessage,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                });
                await unitOfWork.CommitAsync();
                return new ManagerBaseResponse<bool>()
                {
                    Message = "SMS Transaction Log added successfully.",
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
        public async void GetDailySentSMSCount()
        {
            DateTime startDateTime = DateTime.Today.AddDays(-1); //Today at 00:00:00
            DateTime endDateTime = DateTime.Today.AddTicks(-1); //Today at 23:59:59
            var result = unitOfWork
                        .SMSTransactionLogRepository
                        .GetQueryable()
                        .Where(x => !x.IsDeleted && x.CreatedOn >= startDateTime.ToUniversalTime()
                                             && x.CreatedOn <= endDateTime.ToUniversalTime())
                        .ToList();
            var successCount = result.Where(x => x.IsSuccess).Count();
            var failedCount = result.Where(x => !x.IsSuccess).Count();
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/DailySmsSentCountReminder.html"));
            htmlContent = htmlContent.Replace("{Date}", startDateTime.ToString("dd-MM-yyyy"));
            htmlContent = htmlContent.Replace("{SuccessCount}", successCount.ToString());
            htmlContent = htmlContent.Replace("{FailedCount}", failedCount.ToString());
            htmlContent = htmlContent.Replace("{TotalCount}", result.Count().ToString());
            string[] emailIds = configuration["SendContactusMessage"].ToString().Split(',', ' ');
            foreach (var email in emailIds)
            {
                await emailSenderService.SendMail(email, "Melbeez: SMS information", htmlContent, null, null);
            }
        }
    }
}