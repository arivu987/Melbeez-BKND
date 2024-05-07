using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ISMSTransactionLogManager
	{
		Task<ManagerBaseResponse<IEnumerable<SMSTransactionLogResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria);
		Task<ManagerBaseResponse<bool>> AddSMSTransactionLog(SMSTransactionLogResponseModel model, string userId);
        public void GetDailySentSMSCount();
    }
}