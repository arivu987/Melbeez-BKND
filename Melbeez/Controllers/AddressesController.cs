using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
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
    /// This endpoint contains CRUD operation of User's address
    /// </summary>
    [Route("api/addresses")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class AddressesController : BaseController
    {
        private readonly IAddressesManager _addressesManager;
        public AddressesController(IAddressesManager addressesManager)
        {
            _addressesManager = addressesManager;
        }

        /// <summary>
        /// Return list of user's addresses
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<AddressResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAddresses()
        {
            try
            {
                var res = await _addressesManager.Get(User.Claims.GetUserId());
                return ResponseResult(new ManagerBaseResponse<IEnumerable<AddressResponseModel>>()
                {
                    IsSuccess = true,
                    Result = res.Result
                });
            }
            catch (Exception ex)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Add or update user's addresses
        /// </summary>
        /// <param name="model">TypeOfProperty will be - Residential = 1, Billing = 2</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddAddress([FromBody] List<AddressesRequestModel> model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
            var response = await _addressesManager.AddUpdateAddress(model, User.Claims.GetUserId());
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Result = response.Result,
                Message = response.Message
            });
        }
    }
}
