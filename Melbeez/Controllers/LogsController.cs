using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Models;
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
    /// This endpoint contains get operation of logs
    /// </summary>
    [Route("api/log")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class LogsController : BaseController
    {
        private readonly IEmailTransactionLogManager emailTransactionLogManager;
        private readonly ISMSTransactionLogManager smsTransactionLogManager;
        private readonly IAdminTransactionLogManager adminTransactionLogManager;
        private readonly ISMLErrorLogManager smlErrorLogManager;
        private readonly IBarCodeTransactionLogsManager barCodeTransactionLogsManager;
        private readonly IAPIActivitiesManager apiActivitiesManager;
        public LogsController(IEmailTransactionLogManager emailTransactionLogManager
                            , ISMSTransactionLogManager smsTransactionLogManager
                            , IAdminTransactionLogManager adminTransactionLogManager
                            , ISMLErrorLogManager smlErrorLogManager
                            , IBarCodeTransactionLogsManager barCodeTransactionLogsManager
                            , IAPIActivitiesManager apiActivitiesManager)
        {
            this.emailTransactionLogManager = emailTransactionLogManager;
            this.smsTransactionLogManager = smsTransactionLogManager;
            this.adminTransactionLogManager = adminTransactionLogManager;
            this.smlErrorLogManager = smlErrorLogManager;
            this.barCodeTransactionLogsManager = barCodeTransactionLogsManager;
            this.apiActivitiesManager = apiActivitiesManager;
        }

        /// <summary>
        /// Get all email transaction logs
        /// </summary>
        /// <returns></returns>
        [HttpGet("email-transaction-logs")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<EmailTransactionLogResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> EmailTransactionGet([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await emailTransactionLogManager.Get(startDate, endDate, pagedListCriteria));
        }

        /// <summary>
        /// Get all sms transaction logs
        /// </summary>
        /// <returns></returns>
        [HttpGet("sms-transaction-logs")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<SMSTransactionLogResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SMSTransactionGet([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await smsTransactionLogManager.Get(startDate, endDate, pagedListCriteria));
        }

        /// <summary>
        /// Get all admin transaction logs
        /// </summary>
        /// <returns></returns>
        [HttpGet("admin-transaction-logs")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<AdminTransactionLogResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AdminTransactionGet([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await adminTransactionLogManager.Get(startDate, endDate, pagedListCriteria));
        }

        /// <summary>
        /// Get sml error logs
        /// </summary>
        /// <returns></returns>
        [HttpGet("SML-Error-logs")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<SMLErrorLogResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSMLErrorLog([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await smlErrorLogManager.Get(startDate, endDate, pagedListCriteria));
        }

        /// <summary>
        /// Add sml error log
        /// </summary>
        /// <returns></returns>
        [HttpPost("SML-Error-log")]
        [ProducesResponseType(typeof(ApiBasePageResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddSMLErrorLog(SMLErrorLogResponseModel model)
        {
            return ResponseResult(await smlErrorLogManager.Add(model, User.Claims.GetUserId()));
        }

        // <summary>
        /// Add barcode api log
        /// </summary>
        /// <returns></returns>
        [HttpPost("barcode-transaction-log")]
        [ProducesResponseType(typeof(ApiBasePageResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddBarCodeAPILog(BarCodeTransactionLogsRequestModel model)
        {
            return ResponseResult(await barCodeTransactionLogsManager.Add(model, User.Claims.GetUserId()));
        }

        // <summary>
        /// Get api activity log
        /// </summary>
        /// <returns></returns>
        [HttpGet("api-activity-log")]
        [ProducesResponseType(typeof(ApiBasePageResponse<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetApiActivityLog([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (endDate.HasValue)
            {
                endDate = endDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await apiActivitiesManager.Get(startDate, endDate, pagedListCriteria));
        }
    }
}
