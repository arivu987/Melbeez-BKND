using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using System;
using Melbeez.Business.Models.UserModels.RequestModels;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains CRUD operation of eula
    /// </summary>
    [Route("api/eula")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class EulaController : BaseController
    {
        private readonly IEulaManager eulaManager;
        public EulaController(IEulaManager eulaManager)
        {
            this.eulaManager = eulaManager;
        }

        /// <summary>
        /// Get Eula
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadFile()
        {
            var result = await eulaManager.Get();
            return new ContentResult
            {
                Content = result.Result,
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Get Eula - Web
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("web")]
        [ProducesResponseType(typeof(ApiBasePageResponse<CookiePolicyRequestModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEulaForWeb()
        {
            return ResponseResult(await eulaManager.GetEulaForWeb());
        }

        /// <summary>
        /// Update Eula
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<CookiePolicyRequestModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WriteInFile([FromBody] CookiePolicyRequestModel model)
        {
            return ResponseResult(await eulaManager.WrtieInFile(model, User.Claims.GetUserId()));
        }
    }
}
