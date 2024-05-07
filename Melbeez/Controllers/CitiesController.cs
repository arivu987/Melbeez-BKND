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
    /// This endpoint contains CRUD operation of cities
    /// </summary>
    [Route("api/cities")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class CitiesController : BaseController
    {
        private readonly ICitiesManager _citiesManager;
        public CitiesController(ICitiesManager citiesManager)
        {
            _citiesManager = citiesManager;
        }

        /// <summary>
        /// Get all cities
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<CitiesResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCity([FromQuery] PagedListCriteria pagedListCriteria)
        {
            try
            {
                return ResponseResult(await _citiesManager.Get(pagedListCriteria));
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
        /// Add a new city
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        public async Task<IActionResult> AddCity([FromBody] CitiesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _citiesManager.AddCity(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update an existing city
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCity([FromBody] CitiesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _citiesManager.UpdateCity(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Delete a city by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCity([FromRoute] long id)
        {
            if (id == 0)
            {
                throw new Exception("Please provide vaild city id.");
            }

            return ResponseResult(await _citiesManager.DeleteCity(id, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Get city by state
        /// </summary>
        /// <param name="stateId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpGet]
        [Route("by-state/{stateId:int}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<CitiesResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCityByStateId([FromRoute] long stateId)
        {
            try
            {
                return ResponseResult(await _citiesManager.GetCityByStateId(stateId));
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
        /// Get city by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<CitiesResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCityById([FromRoute] long id)
        {
            try
            {
                return ResponseResult(await _citiesManager.GetCityById(id));
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
    }
}
