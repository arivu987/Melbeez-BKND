using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models;
using Melbeez.Business.Models.Common;
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
    /// This endpoint contains the CRUD operation of register device
    /// </summary>
    [Route("api/register-device")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class RegisterDeviceController : BaseController
    {
        private readonly IRegisterDeviceManager _registerDeviceManager;
        public RegisterDeviceController(IRegisterDeviceManager registerDeviceManager)
        {
            _registerDeviceManager = registerDeviceManager;
        }

        /// <summary>
        /// Register a new device
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("")]
        public async Task<IActionResult> Add([FromBody] RegisterDeviceModel model)
        {
            return ResponseResult(await _registerDeviceManager.AddRegisterDevice(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Delete register device
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpDelete("{uid}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete([FromRoute] string uid)
        {
            if (string.IsNullOrEmpty(uid))
            {
                throw new Exception("Requested uid is not valid.");
            }
            return ResponseResult(await _registerDeviceManager.DeleteRegisterDevice(uid, User.Claims.GetUserId()));
        }
    }
}