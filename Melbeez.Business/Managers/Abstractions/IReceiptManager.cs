using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IReceiptManager
	{
		Task<ManagerBaseResponse<IEnumerable<ReceiptResponseModel>>> Get(DateTime? purchaseFromDate, DateTime? purchaseToDate, PagedListCriteria pagedListCriteria, string userId);
		Task<ManagerBaseResponse<IEnumerable<ReceiptDetailResponseModel>>> Get(long id, string userId);
		Task<ManagerBaseResponse<bool>> AddReceipt(ReceiptRequestModel model, string userId);
		Task<ManagerBaseResponse<bool>> UpdateReceipt(ReceiptRequestModel model, string userId);
		Task<ManagerBaseResponse<bool>> DeleteReceipt(long id, string userId);
	}
}