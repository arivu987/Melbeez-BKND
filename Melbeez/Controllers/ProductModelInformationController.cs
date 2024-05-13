using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
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
    /// This endpoint contains CRUD operation of product model information
    /// </summary>
    [Route("api/productModelInformation")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class ProductModelInformationController : BaseController
    {
        private readonly IProductModelInformationManager _productModelInformationManager;
        public ProductModelInformationController(IProductModelInformationManager productModelInformationManager)
        {
            _productModelInformationManager = productModelInformationManager;
        }

        /// <summary>
        /// Get search products model info
        /// </summary>
        /// <param name="modelNumber"></param>
        /// <param name="manufacturerName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("search")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductModelInfoResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductsModelInfo(string modelNumber, string manufacturerName)
        {
            return ResponseResult(await _productModelInformationManager.GetProductsModelInfo(modelNumber, manufacturerName));
        }

        /// <summary>
        /// Get all products model info
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductModelInfoResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(ProductModelStatus? status, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            return ResponseResult(await _productModelInformationManager.Get(status, pagedListCriteria));
        }

        /// <summary>
        /// Update an existing product model information
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="status"> Status : 0-Pending, 1-Approved, 2-Submitted, 3-Rejected</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("approve-or-reject")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProductModelStatus(List<long> ids, ProductModelStatus status)
        {
            if (!ids.Any())
            {
                throw new Exception("Please provide vaild information.");
            }

            return ResponseResult(await _productModelInformationManager.UpdateProductModelStatus(ids, status, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Delete a product model information by ids
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteProductModelInfo(long id)
        {
            if (id == 0)
            {
                throw new Exception("Requested id is not valid.");
            }

            return ResponseResult(await _productModelInformationManager.DeleteProductModelInfo(id, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Create a new product model information
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddProductModelInfo(ProductModelInfoRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _productModelInformationManager.AddProductModelInfo(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Update product model information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiBaseResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProductModelInfo([FromRoute] long id, ProductModelInfoRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _productModelInformationManager.UpdateProductModelInfo(id, model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Upload bulk product model information
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("bulk-upload")]
        [ProducesResponseType(typeof(ApiBaseResponse<List<BulkUploadInsertedRowStatusResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UploadBulkData(IFormFile file)
        {
            return ResponseResult(await _productModelInformationManager.UploadBulkProductModels(file, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Search model number and Manufacturer Name
        /// </summary>
        /// <param name="modelNumber"></param>
        /// <param name="manufacturerName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("search/model-manufacturer")]
        [ProducesResponseType(typeof(ApiBaseResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> IsModelAndManufacturerAlreadyExists(string manufacturerName, string modelNumber)
        {
            return ResponseResult(await _productModelInformationManager.IsModelAndManufacturerAlreadyExists(manufacturerName, modelNumber));
        }

        /// <summary>
        /// Get model number and Manufacturer Name
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("manufacturer-and-model/{manufacturerName}")]
        [ProducesResponseType(typeof(ApiBaseResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetManufacturerAndModelNumber([FromRoute] string manufacturerName)
        {
            return ResponseResult(await _productModelInformationManager.GetManufacturerModelNumber(manufacturerName));
        }

        /// <summary>
        /// Processes image data to extract model number and manufacturer name using OCR.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("process-image")]
        [RequestFormLimits(MultipartBodyLengthLimit = 629145600)]
        [ProducesResponseType(typeof(ApiBaseResponse<ProcessImageResponseModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ProcessImageWithOCR(IFormFile image)
        {
            return ResponseResult(await _productModelInformationManager.ProcessImageWithOCR(image));
        }
    }
}
