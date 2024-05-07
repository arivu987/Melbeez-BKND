using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models;
using Melbeez.Business.Models.Common;
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
    /// This endpoint contains CRUD operation of countries
    /// </summary>
    [Route("api/countries")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class CountriesController : BaseController
    {
        private readonly ICountryManager _countryManager;
        public CountriesController(ICountryManager countryManager)
        {
            _countryManager = countryManager;
        }
        /// <summary>
        /// Get all countries
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<CountryViewModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCountry([FromQuery] PagedListCriteria pagedListCriteria)
        {
            try
            {
                return ResponseResult(await _countryManager.Get(pagedListCriteria));
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
        /// Add a new country
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddCountry([FromBody] CountriesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _countryManager.AddCountry(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update an existing counrty
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCountry([FromBody] CountriesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
            
            return ResponseResult(await _countryManager.UpdateCountry(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Delete a country by id
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
                throw new Exception("Please provide vaild country id.");
            }
          
            return ResponseResult(await _countryManager.DeleteCountry(id, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Get country by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<CountryViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCountry([FromRoute] long id)
        {
            try
            {
                return ResponseResult(await _countryManager.Get(id));
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
