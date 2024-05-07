using Melbeez.Business.Managers;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Data.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using Melbeez.Common.Helpers;

namespace Melbeez.Controllers
{
    /// <summary>
    /// This endpoint contains tranfer operation of an item
    /// </summary>
    [Route("api/item-transfer")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class ItemTransferController : BaseController
    {
        private readonly IItemTransferManager itemTransferManager;
        public ItemTransferController(IItemTransferManager itemTransferManager)
        {
            this.itemTransferManager = itemTransferManager;
        }

        /// <summary>
        /// Get Sender to transfer an item 
        /// </summary>        
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("sender-transfer-item")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransferItem()
        {
            return ResponseResult(await itemTransferManager.GetTransferItem(User.Claims.GetUserId(),false,null));
        }

        /// <summary>
        /// Get transfer an item for Receiver
        /// </summary>        
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("receive-transfer-item")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecevieTransferItem(MovedStatus? status)
        {
            return ResponseResult(await itemTransferManager.GetTransferItem(User.Claims.GetUserId(), true, status));
        }

        /// <summary>
        /// Request to transfer an item 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> TransferItem(ItemTransferRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await itemTransferManager.TransferItem(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Request to cancel or reject transfer an item 
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="movedStatus"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("cancelOrReject-transfer-item")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelTransferItem(string transferId, MovedStatus movedStatus)
        {
            return ResponseResult(await itemTransferManager.CancelOrRejectTransferItem(transferId, movedStatus, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Request to approve transfer an item 
        /// </summary>
        /// <param name="transferId"></param>
        /// <param name="IsSameLocation"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("approve-transfer-item")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApproveTransferItem(string transferId, bool IsSameLocation, long? locationId)
        {
            return ResponseResult(await itemTransferManager.ApproveTransferItem(transferId, User.Claims.GetUserId(), IsSameLocation, locationId));
        }

        /// <summary>
        /// Request to delete transfer an item 
        /// </summary>
        /// <param name="transferId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("delete-transfer-item")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteTransferItem(string transferId)
        {
            return ResponseResult(await itemTransferManager.DeleteTransferItem(transferId, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Get transfered item history
        /// </summary>        
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("transfered-item-history")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTransferedItems()
        {
            return ResponseResult(await itemTransferManager.GetTransferedItems(User.Claims.GetUserId()));
        }

    }
}
