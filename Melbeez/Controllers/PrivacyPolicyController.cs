using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains CRUD operation of privacy policy
    /// </summary>
    [Route("api/privacy-policy")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class PrivacyPolicyController : BaseController
    {
        private readonly IPrivacyPolicyManager privacyPolicyManager;
        public PrivacyPolicyController(IPrivacyPolicyManager privacyPolicyManager)
        {
            this.privacyPolicyManager = privacyPolicyManager;
        }

        /// <summary>
        /// Get privacy policy
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadFile()
        {
            var result = await privacyPolicyManager.Get();
            return new ContentResult
            {
                Content = result.Result,
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Get privacy policy - Web
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("web")]
        [ProducesResponseType(typeof(ApiBasePageResponse<PrivacyPolicyRequestModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrivacyPolicyForWeb()
        {
            return ResponseResult(await privacyPolicyManager.GetPrivacyPolicyForWeb());
        }

        /// <summary>
        /// Update privacy policy
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<PrivacyPolicyRequestModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WriteInFile([FromBody] PrivacyPolicyRequestModel model)
        {
            return ResponseResult(await privacyPolicyManager.WrtieInFile(model, User.Claims.GetUserId()));
        }
    }
}
