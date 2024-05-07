using Melbeez.Business.Models.Common;
using Melbeez.Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IBarCodeTransactionLogsManager
    {
        Task<ManagerBaseResponse<IEnumerable<BarCodeTransactionLogsResponseModel>>> Get();
        Task<ManagerBaseResponse<bool>> Add(BarCodeTransactionLogsRequestModel model, string userId);
    }
}
