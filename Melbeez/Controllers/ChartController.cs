using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains dashborad charts 
    /// </summary>
    [Route("api/chart")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class ChartController : BaseController
    {
        private readonly IChartManager chartManager;
        public ChartController(IChartManager chartManager)
        {
            this.chartManager = chartManager;
        }
        /// <summary>
        /// Get Chart
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<DashboardChartResponseModel>), StatusCodes.Status200OK)]
        public IActionResult GetChart()
        {
            try
            {
                return ResponseResult(chartManager.Get(User.Claims.GetUserId()));
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
        /// Get Admin Dashboard Chart
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet("admin-dashboard")]
        [ProducesResponseType(typeof(ApiBasePageResponse<AdminDashboardChartResponseModel>), StatusCodes.Status200OK)]
        public IActionResult GetAdminDashboardChart([FromQuery] FilterGraphModel filterGraphModel)
        {
            try
            {
                return ResponseResult(chartManager.GetAdminDashboardChart(filterGraphModel));
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
        /// Get Report
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet("Report")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public IActionResult GetReport([FromQuery] FilterGraphModel filterGraphModel)
        {
            return ResponseResult(chartManager.ExportToExcel(filterGraphModel));
        }
    }
}
