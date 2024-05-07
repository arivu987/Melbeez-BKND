using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class UsersActivityTransactionLogManager : IUsersActivityTransactionLogManager
    {
        private readonly IUnitOfWork unitOfWork;

        public UsersActivityTransactionLogManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<IEnumerable<UsersActivityTransactionLogResponseModel>>> GetActiveUsers()
        {
            var result = await unitOfWork.UsersActivityTransactionLogRepository.GetQueryable(x => !x.IsDeleted)
                        .Select(x => new UsersActivityTransactionLogResponseModel()
                        {
                            Id = x.Id,
                            UserId = x.CreatedBy,
                            IPAddress = x.IPAddress,
                            ActiveDate = x.CreatedOn
                        }).ToListAsync();
            return new ManagerBaseResponse<IEnumerable<UsersActivityTransactionLogResponseModel>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> AddTransactionLog(UsersActivityTransactionLogResponseModel model)
        {
            try
            {
                await unitOfWork.UsersActivityTransactionLogRepository.AddAsync(new UsersActivityTransactionLogEntity()
                {
                    IPAddress = model.IPAddress,
                    CreatedBy = model.UserId,
                    CreatedOn = model.ActiveDate,
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
