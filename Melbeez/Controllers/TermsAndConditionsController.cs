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
    /// This endpoint contains CRUD operation of terms and condition
    /// </summary>
    [Route("api/termsandconditions")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class TermsAndConditionsController : BaseController
    {
        private readonly ITermsAndConditionsManager termsAndConditionsManager;
        public TermsAndConditionsController(ITermsAndConditionsManager termsAndConditionsManager)
        {
            this.termsAndConditionsManager = termsAndConditionsManager;
        }

        /// <summary>
        /// Get terms and conditions
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadFile()
        {
            var result = await termsAndConditionsManager.Get();
            return new ContentResult
            {
                Content = result.Result,
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        /// <summary>
        /// Get terms and conditions - Web
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("web")]
        [ProducesResponseType(typeof(ApiBasePageResponse<PrivacyPolicyRequestModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTermAndConditioonsForWeb()
        {
            return ResponseResult(await termsAndConditionsManager.GetTermAndConditionsForWeb());
        }

        /// <summary>
        /// Update terms and conditions
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<PrivacyPolicyRequestModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> WriteInFile([FromBody] TermsAndConditionsRequestModel model)
        {
            return ResponseResult(await termsAndConditionsManager.WrtieInFile(model, User.Claims.GetUserId()));
        }
    }
}
