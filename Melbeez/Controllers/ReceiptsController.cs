using DocumentFormat.OpenXml.Wordprocessing;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
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
    /// This endpoint contains CRUD operation of receipts
    /// </summary>
    [Route("api/receipts")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class ReceiptsController : BaseController
    {
        private readonly IReceiptManager _receiptManager;
        public ReceiptsController(IReceiptManager receiptManager)
        {
            _receiptManager = receiptManager;
        }

        /// <summary>
        /// Get all receipts
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ReceiptResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReceipts(DateTime? purchaseFromDate, DateTime? purchaseToDate, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (purchaseToDate.HasValue)
            {
                purchaseToDate = purchaseToDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await _receiptManager.Get(purchaseFromDate, purchaseToDate, pagedListCriteria, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Get receipt by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ReceiptDetailResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReceipts([FromRoute] long id)
        {
            return ResponseResult(await _receiptManager.Get(id, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Create a new receipt
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddReceipt(ReceiptRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _receiptManager.AddReceipt(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update an existing receipt
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateReceipt(ReceiptRequestModel model)
        {
            return ResponseResult(await _receiptManager.UpdateReceipt(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Delete a receipt by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteReceipt([FromRoute] long id)
        {
            return ResponseResult(await _receiptManager.DeleteReceipt(id, User.Claims.GetUserId()));
        }
    }
}
