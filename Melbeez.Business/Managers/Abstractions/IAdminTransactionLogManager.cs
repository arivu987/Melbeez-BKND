using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IAdminTransactionLogManager
    {
        Task<ManagerBaseResponse<IEnumerable<AdminTransactionLogResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria);
        Task<ManagerBaseResponse<bool>> AddTransactionLog(AdminTransactionLogResponseModel model, string userId);
    }
}
