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
    internal class APIDownStatusManager : IAPIDownStatusManager
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly Dictionary<string, string> orderByTranslations = new Dictionary<string, string>();
        public APIDownStatusManager(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
        }

        public async Task<ManagerBaseResponse<IEnumerable<APIDownStatusResponseModel>>> Get(PagedListCriteria pagedListCriteria)
        {
            var result = await unitOfWork
                .APIDownStatusRepository
                .GetQueryable(x => !x.IsDeleted)
                .Select(x => new APIDownStatusResponseModel()
                {
                    Id = x.Id,
                    IsAPIDown = x.IsApiDown,
                    Status = x.IsApiDown == true ? "Down state" : "Maintained state",
                    CreatedBy = string.Concat(userManager.Users.FirstOrDefault(a => a.Id == x.CreatedBy).FirstName, " ",
                                              userManager.Users.FirstOrDefault(a => a.Id == x.CreatedBy).LastName),
                    CreatedOn = x.CreatedOn
                })
                .AsNoTracking()
                .ToPagedListAsync(pagedListCriteria, orderByTranslations);
            return new ManagerBaseResponse<IEnumerable<APIDownStatusResponseModel>>()
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

        public async Task<bool> GetLastApiStatusData()
        {
            var result = unitOfWork
                        .APIDownStatusRepository
                        .GetQueryable(x => !x.IsDeleted)
                        .OrderByDescending(x => x.CreatedOn)
                        .FirstOrDefault();
            if (result != null)
            {
                return result.IsApiDown;
            }
            return false;
        }
        public async Task<ManagerBaseResponse<bool>> Add(bool isAPIDown, string userId)
        {
            try
            {
                var result = unitOfWork
                        .APIDownStatusRepository
                        .GetQueryable(x => !x.IsDeleted)
                        .OrderByDescending(x => x.CreatedOn)
                        .FirstOrDefault();

                if (result != null && result.IsApiDown == isAPIDown)
                {
                    return new ManagerBaseResponse<bool>()
                    {
                        Message = "System already in " + (isAPIDown ? "down" : "maintained") + " state.",
                        Result = false
                    };
                }

                await unitOfWork.APIDownStatusRepository.AddAsync(new APIDownStatusEntity()
                {
                    IsApiDown = isAPIDown,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                });
                await unitOfWork.CommitAsync();
                return new ManagerBaseResponse<bool>()
                {
                    Message = "System in " + (isAPIDown ? "down" : "maintained") + " state.",
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
