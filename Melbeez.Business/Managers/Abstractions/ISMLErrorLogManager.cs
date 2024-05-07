using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ISMLErrorLogManager
	{
		Task<ManagerBaseResponse<IEnumerable<SMLErrorLogResponseModel>>> Get(DateTime? startDate, DateTime? endDate, PagedListCriteria pagedListCriteria);
		Task<ManagerBaseResponse<string>> Add(SMLErrorLogResponseModel model, string userId);
	}
}