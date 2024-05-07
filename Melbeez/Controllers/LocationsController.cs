using DocumentFormat.OpenXml.Wordprocessing;
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
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains CRUD operation of product's location
    /// </summary>
    [Route("api/locations")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class LocationsController : BaseController
    {
        private readonly ILocationsManager _locationsManager;
        public LocationsController(ILocationsManager LocationsManager)
        {
            _locationsManager = LocationsManager;
        }

        /// <summary>
        /// Get all locations
        /// </summary>
        /// <returns></returns>
        /// <param name="locationTypes"></param>
        /// <param name="categoryId"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="pagedListCriteria"></param>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<LocationsResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLocation(string locationTypes, string categoryId, DateTime? from, DateTime? to, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            try
            {
                if (to.HasValue)
                {
                    to = to.Value.AddDays(1).AddTicks(-1);
                }
                return ResponseResult(await _locationsManager.Get(locationTypes, categoryId, from, to, pagedListCriteria, User.Claims.GetUserId()));
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
        /// Get location by id
        /// </summary>
        /// <returns></returns>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<LocationsResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLocation([FromRoute] long id)
        {
            try
            {
                return ResponseResult(await _locationsManager.Get(id, User.Claims.GetUserId()));
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
        /// Add a new location
        /// </summary>
        /// <param name="model">TypeOfProperty will be - Residential = 1, Office = 2, Retail = 3</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseResponse<long?>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddLocation([FromBody] LocationsRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
            return ResponseResult(await _locationsManager.AddLocation(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update an existing location
        /// </summary>
        /// <param name="model">TypeOfProperty will be - Residential = 1, Office = 2, Retail = 3</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateLocation([FromBody] LocationsRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
            return ResponseResult(await _locationsManager.UpdateLocation(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Delete location by Ids
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Delete(List<long> Id)
        {
            if (Id.Count() == 0)
            {
                throw new Exception("Requested id is not valid.");
            }
            return ResponseResult(await _locationsManager.DeleteLocation(Id, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Get location by user id
        /// </summary>
        /// <returns></returns>
        /// <param name="userId"></param>
        /// <exception cref="Exception"></exception>
        [HttpGet("get-by-userId/{userId}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<LocationsResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLocationByUserId([FromRoute] string userId)
        {
            try
            {
                return ResponseResult(await _locationsManager.GetLocationByUserId(userId));
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
