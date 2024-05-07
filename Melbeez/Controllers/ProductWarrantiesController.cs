using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Entities;
using Melbeez.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains CRUD operation of Warranties
    /// </summary>
    [Route("api/products/warranties")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class ProductWarrantiesController : BaseController
    {
        private readonly IProductWarrantiesManager _productWarrantiesManager;
        public ProductWarrantiesController(IProductWarrantiesManager productWarrantiesManager)
        {
            _productWarrantiesManager = productWarrantiesManager;
        }

        /// <summary>
        /// Get all warranties
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<BaseWarrantiesResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(WarrantyStatus? status, string categories, DateTime? warrantyFromDate, DateTime? warrantyToDate, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (warrantyToDate.HasValue)
            {
                warrantyToDate = warrantyToDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await _productWarrantiesManager.Get(status, categories, warrantyFromDate, warrantyToDate, pagedListCriteria, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Get warranties by productId
        /// </summary>
        /// <param name="productid"></param>
        /// <returns></returns>
        [HttpGet("/api/products/{productid}/warranties")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductWarrantiesResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromRoute] long productid)
        {
            return ResponseResult(await _productWarrantiesManager.Get(productid, null, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Create a new warranty
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(List<ProductWarrantiesRequestModel> model)
        {
            return ResponseResult(await _productWarrantiesManager.AddWarranty(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update an existing warranty
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Put(List<ProductWarrantiesRequestModel> model)
        {
            return ResponseResult(await _productWarrantiesManager.UpdateWarranty(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Delete a warranty by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            return ResponseResult(await _productWarrantiesManager.Delete(id, User.Claims.GetUserId()));
        }
    }
}
