using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
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
    /// This endpoint contains CRUD operation of states
    /// </summary>
    [Route("api/states")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class StatesController : BaseController
    {
        private readonly IStatesManager _statesManager;
        public StatesController(IStatesManager statesManager)
        {
            _statesManager = statesManager;
        }

        /// <summary>
        /// Get all states
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<StatesResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetState([FromQuery] PagedListCriteria pagedListCriteria)
        {
            try
            {
                return ResponseResult(await _statesManager.Get(pagedListCriteria));
            }
            catch (Exception ex)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = ex.Message,
                    StatusCode = 500
                });
            }
        }
        /// <summary>
        /// Add a new state
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddCity([FromBody] StatesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _statesManager.AddState(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update an existing state
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateState([FromBody] StatesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _statesManager.UpdateState(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Delete a state by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteState([FromRoute] long id)
        {
            if (id == 0)
            {
                throw new Exception("Please provide vaild state id.");
            }
            
            return ResponseResult(await _statesManager.DeleteState(id, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Get state by countryId
        /// </summary>
        /// <param name="countryId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>

        [HttpGet("by-country/{countryId}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<StatesResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatebyCountryId([FromRoute] long countryId)
        {
            try
            {
                return ResponseResult(await _statesManager.GetStateByCountryId(countryId));
            }
            catch (Exception ex)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = ex.Message,
                    StatusCode = 500
                });
            }
        }
        /// <summary>
        /// Get state by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<StatesResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStatebyId([FromRoute] long id)
        {
            try
            {
                return ResponseResult(await _statesManager.GetStateById(id));
            }
            catch (Exception ex)
            {
                return ResponseResult(new ManagerBaseResponse<bool>()
                {
                    IsSuccess = false,
                    Result = false,
                    Message = ex.Message,
                    StatusCode = 500
                });
            }
        }
    }
}
