using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IItemTransferManager
    {
        Task<ManagerBaseResponse<List<TransferItemResponse>>> GetTransferItem(string userId, bool isRecevier, MovedStatus? status);
        Task<ManagerBaseResponse<bool>> TransferItem(ItemTransferRequestModel model, string userId);
        Task<ManagerBaseResponse<bool>> CancelOrRejectTransferItem(string transferId, MovedStatus movedStatus, string userId);
        Task<ManagerBaseResponse<bool>> ApproveTransferItem(string transferId, string userId, bool IsSameLocation, long? locationId);
        Task<ManagerBaseResponse<bool>> DeleteTransferItem(string transferId, string userId);
        Task<ManagerBaseResponse<List<ItemTransferedHistoryResponse>>> GetTransferedItems(string userId);
        Task<ManagerBaseResponse<bool>> SetItemExpriedStatus();
    }
}
