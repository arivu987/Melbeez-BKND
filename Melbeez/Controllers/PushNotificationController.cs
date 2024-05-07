using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
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
    /// This endpoint contains GET operation of notification
    /// </summary>
    [Route("api/pushnotification")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class PushNotificationController : BaseController
    {
        private readonly IPushNotificationManager _pushNotificationManager;
        private readonly ISendNotificationManager _sendNotificationManager;
        private readonly IUserNotificationPreferenceManager _userNotificationPreferenceManager;
        public PushNotificationController(IPushNotificationManager pushNotificationManager,
                                          ISendNotificationManager sendNotificationManager,
                                          IUserNotificationPreferenceManager userNotificationPreferenceManager)
        {
            _pushNotificationManager = pushNotificationManager;
            _sendNotificationManager = sendNotificationManager;
            _userNotificationPreferenceManager = userNotificationPreferenceManager;
        }

        /// <summary>
        /// Get notifications
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<PushNotificationModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotification([FromQuery] PagedListCriteria pagedListCriteria)
        {
            try
            {
                return ResponseResult(await _pushNotificationManager.Get(pagedListCriteria, User.Claims.GetUserId()));
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
        /// Send Notification
        /// </summary>
        /// <param name="model"> NotificationType will be - WarrantyExpiry = 1, LocationUpdate = 2, ProductUpdate = 3, DeviceActivation = 4, MarketingAlert = 5 </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendNotification(PushNotificationRequestModel model)
        {
            if (model.NotificationType == NotificationType.MarketingAlert)
            {
                return ResponseResult(await _sendNotificationManager.SendMarketingAlertNotification(model, User.Claims.GetUserId()));
            }
            return ResponseResult(new ManagerBaseResponse<bool>()
            {
                Result = false,
                Message = "Notification Type doesn't match",
                StatusCode = 500
            });
        }

        /// <summary>
        /// Read Notification
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("read-notification")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadNotification(PushNotificationReadRequestModel model)
        {
            return ResponseResult(await _pushNotificationManager.ReadNotification(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Delete Notification
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteNotification(List<long?> ids)
        {
            return ResponseResult(await _pushNotificationManager.DeleteNotification(ids, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Get user notifications preference
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("user-notification-preference")]
        [ProducesResponseType(typeof(ApiBasePageResponse<UserNotificationPreferenceResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotificationPreference()
        {
            try
            {
                return ResponseResult(await _userNotificationPreferenceManager.Get(User.Claims.GetUserId()));
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
        /// Add user notification preference
        /// </summary>
        /// <param name="model"> </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("user-notification-preference")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddNotificationPreference(UserNotificationPreferenceResponseModel model)
        {
            return ResponseResult(await _userNotificationPreferenceManager.AddUserNotificationPreference(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update user notification preference
        /// </summary>
        /// <param name="model"> </param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("user-notification-preference")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateNotificationPreference(UserNotificationPreferenceResponseModel model)
        {
            return ResponseResult(await _userNotificationPreferenceManager.UpdateUserNotificationPreference(model, User.Claims.GetUserId()));
        }
    }
}
