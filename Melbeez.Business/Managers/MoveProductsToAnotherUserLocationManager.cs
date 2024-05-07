using DocumentFormat.OpenXml.Spreadsheet;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class MoveProductsToAnotherUserLocationManager : IMoveProductsToAnotherUserLocationManager
    {
        private readonly IUnitOfWork unitOfWork;
        public MoveProductsToAnotherUserLocationManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<ManagerBaseResponse<MoveItemsResponse>> GetTransferedItems(long productId, string userId)
        {
            return new ManagerBaseResponse<MoveItemsResponse>()
            {
                Result = null
            };
        }
        public async Task<ManagerBaseResponse<long>> AddTransferedItem(AddMoveProductsRequestModel model, string FromUserId)
        {
            try
            {
                var insertedId = await unitOfWork
                                .MoveProductsToAnotherUserLocationRepository
                                .AddAsync(new MovedItemStatusTransactonsEntity()
                                {
                                    FromUserId = FromUserId,
                                    ToUserId = model.ToUserId,
                                    Status = MovedStatus.Waiting,
                                    IsDeleted = false,
                                    CreatedBy = FromUserId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = FromUserId,
                                    UpdatedOn = DateTime.UtcNow
                                });
                await unitOfWork.CommitAsync();

                return new ManagerBaseResponse<long>()
                {
                    Message = "Product move request sent successfully.",
                    Result = insertedId.Id
                };

            }
            catch (Exception ex)
            {
                return new ManagerBaseResponse<long>()
                {
                    Message = ex.Message,
                    Result = 0
                };
            }
        }
    }
}
