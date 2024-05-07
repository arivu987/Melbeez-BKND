using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Models;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class AdminTransactionLogManager : IAdminTransactionLogManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public AdminTransactionLogManager(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        public async Task<ManagerBaseResponse<IEnumerable<AdminTransactionLogResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .AdminTransactionLogRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new AdminTransactionLogResponseModel()
                {
                    TransactionId = x.Id,
                    UserId = x.UserId,
                    TransactionDescription = x.LogDescription,
                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    CreatedBy = string.Concat(userManager.Users.FirstOrDefault(a => a.Id == x.CreatedBy).FirstName, " ",
                                              userManager.Users.FirstOrDefault(a => a.Id == x.CreatedBy).LastName),
                    CreatedOn = x.CreatedOn
                })
                .WhereIf(startDate.HasValue && !(endDate.HasValue), w => w.CreatedOn >= startDate.Value.ToUniversalTime())
                .WhereIf(endDate.HasValue && !(startDate.HasValue), w => w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(startDate.HasValue && endDate.HasValue, w => w.CreatedOn >= startDate.Value.ToUniversalTime() && w.CreatedOn <= endDate.Value.ToUniversalTime())
                .WhereIf(!string.IsNullOrWhiteSpace(pagedListCriteria.SearchText), x => x.TransactionDescription.ToLower().Contains(pagedListCriteria.SearchText.ToLower()) 
                                                                                    || x.CreatedBy.ToLower().Contains(pagedListCriteria.SearchText.ToLower()))
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<AdminTransactionLogResponseModel>>()
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
        public async Task<ManagerBaseResponse<bool>> AddTransactionLog(AdminTransactionLogResponseModel model, string userId)
        {
            try
            {
                await unitOfWork.AdminTransactionLogRepository.AddAsync(new AdminTransactionLogEntity()
                {
                    UserId = model.UserId,
                    LogDescription = model.TransactionDescription,
                    OldStatus = model.OldStatus,
                    NewStatus = model.NewStatus,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                });
                await unitOfWork.CommitAsync();
                return new ManagerBaseResponse<bool>()
                {
                    Message = "Transaction Log added successfully.",
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
