using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
using Melbeez.Data.Identity;
using Melbeez.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains CRUD operation of api status
    /// </summary>
    [Authorize(Roles = "SuperAdmin, Admin")]
    [Route("api/api-status")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class APIDownStatusController : BaseController
    {
        private readonly IAPIDownStatusManager apiDownStatusManager;
        public APIDownStatusController(IAPIDownStatusManager apiDownStatusManager)
        {
            this.apiDownStatusManager= apiDownStatusManager;
        }

        /// <summary>
        /// Get api status
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<APIDownStatusResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> APIDownStatus([FromQuery] PagedListCriteria pagedListCriteria)
        {
            return ResponseResult(await apiDownStatusManager.Get(pagedListCriteria));
        }

        /// <summary>
        /// Make system undergoing maintenance
        /// </summary>
        /// <returns></returns>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddBarCodeAPILog([FromQuery] bool isAPIDown)
        {
            var result = await apiDownStatusManager.Add(isAPIDown, User.Claims.GetUserId());
            if (result != null && result.IsSuccess)
            {
                ApiDownMiddleware.ResetApiDownStatus();
            }
            return ResponseResult(result);
        }
    }
}
