using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IProductImageManager
	{
		Task<ManagerBaseResponse<IEnumerable<ProductImageResponseModel>>> Get();
	}
}