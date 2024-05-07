using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models;
using Melbeez.Business.Models.Common;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class BarCodeTransactionLogsManager : IBarCodeTransactionLogsManager
    {
        private readonly IUnitOfWork unitOfWork;
        public BarCodeTransactionLogsManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<ManagerBaseResponse<IEnumerable<BarCodeTransactionLogsResponseModel>>> Get()
        {
            var result = await unitOfWork
                        .BarCodeTransactionLogsRepository
                        .GetQueryable(x => !x.IsDeleted)
                        .Select(x => new BarCodeTransactionLogsResponseModel()
                        {
                            Id = x.Id,
                            BarCode = x.BarCode,
                            Title = x.Title,
                            Status = x.Status,
                            ErrorMessage = x.ErrorMessage,
                            CreatedBy = x.CreatedBy,
                            CreatedOn = x.CreatedOn
                        })
                        .AsNoTracking()
                        .ToListAsync();
            return new ManagerBaseResponse<IEnumerable<BarCodeTransactionLogsResponseModel>>()
            {
                Result = result
            };
        }
        public async Task<ManagerBaseResponse<bool>> Add(BarCodeTransactionLogsRequestModel model, string userId)
        {
            try
            {
                await unitOfWork.BarCodeTransactionLogsRepository.AddAsync(new BarCodeTransactionLogsEntity()
                {
                    BarCode = model.BarCode,
                    Title = model.Title,
                    Status = model.Status,
                    ErrorMessage = model.ErrorMessage,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                });
                await unitOfWork.CommitAsync();

                return new ManagerBaseResponse<bool>()
                {
                    Message = "Bar Code Transaction Log added successfully.",
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
