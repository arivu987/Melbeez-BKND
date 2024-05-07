using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IUsersActivityTransactionLogManager
    {
        Task<ManagerBaseResponse<IEnumerable<UsersActivityTransactionLogResponseModel>>> GetActiveUsers();
        Task<ManagerBaseResponse<bool>> AddTransactionLog(UsersActivityTransactionLogResponseModel model);
    }
}
