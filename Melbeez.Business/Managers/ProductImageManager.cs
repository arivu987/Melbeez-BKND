using AutoMapper;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Data.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class ProductImageManager : IProductImageManager
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IMapper mapper;

		public ProductImageManager(
			IUnitOfWork unitOfWork,
			IMapper mapper
		)
		{
			this.unitOfWork = unitOfWork;
			this.mapper = mapper;
		}
		public async Task<ManagerBaseResponse<IEnumerable<ProductImageResponseModel>>> Get()
		{
			var result = await unitOfWork
				.ProductImageRepository
				.GetQueryable(x => !x.IsDeleted)
				.Select(x => new ProductImageResponseModel()
				{
					ProductId = x.ProductId,
					ImageUrl = x.ImageUrl,
					FileSize = x.FileSize,
					IsDefault = x.IsDefault,
				})
				.AsNoTracking()
				.ToListAsync();
			return new ManagerBaseResponse<IEnumerable<ProductImageResponseModel>>()
			{
				Result = result
			};
		}
	}
}