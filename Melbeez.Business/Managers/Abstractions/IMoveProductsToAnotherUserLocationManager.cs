using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IMoveProductsToAnotherUserLocationManager
    {
        Task<ManagerBaseResponse<MoveItemsResponse>> GetTransferedItems(long productId, string userId);
        Task<ManagerBaseResponse<long>> AddTransferedItem(AddMoveProductsRequestModel model, string FromUserId);
    }
}
