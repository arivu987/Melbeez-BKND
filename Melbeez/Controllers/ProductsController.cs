using DocumentFormat.OpenXml.Wordprocessing;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
using Melbeez.Common.Models;
using Melbeez.Common.Models.Common;
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
    /// This endpoint contains CRUD operation of products
    /// </summary>
    [Route("api/products")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class ProductsController : BaseController
    {
        private readonly IProductsManager _productsManager;
        public ProductsController(IProductsManager productsManager)
        {
            _productsManager = productsManager;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductsResponseModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts(string locationIds, string categoryIds, DateTime? purchaseFromDate, DateTime? purchaseToDate, DateTime? warrantyFromDate, DateTime? warrantyToDate, bool? isTransferItem, [FromQuery] PagedListCriteria pagedListCriteria)
        {
            if (purchaseToDate.HasValue)
            {
                purchaseToDate = purchaseToDate.Value.AddDays(1).AddTicks(-1);
            }
            if (warrantyToDate.HasValue)
            {
                warrantyToDate = warrantyToDate.Value.AddDays(1).AddTicks(-1);
            }
            return ResponseResult(await _productsManager.Get(locationIds, categoryIds, purchaseFromDate, purchaseToDate, warrantyFromDate, warrantyToDate, isTransferItem, pagedListCriteria, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Get a product by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductFormDataRequestModel>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts(long id)
        {
            return ResponseResult(await _productsManager.Get(id, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Create a new product
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseResponse<long?>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddProduct(ProductRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
           
            return ResponseResult(await _productsManager.AddProduct(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Update an existing product
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProduct([FromRoute] long id, ProductRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
         
            return ResponseResult(await _productsManager.UpdateProduct(id, model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Delete a product by ids
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteProducts(List<long> id)
        {
            if (id.Count() == 0)
            {
                throw new Exception("Requested id is not valid.");
            }
           
            return ResponseResult(await _productsManager.DeleteProduct(id, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Move a product to another location
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("move-location")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MoveProductsLocation(MoveProductsLocationRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
     
            return ResponseResult(await _productsManager.MoveProductsLocation(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Add product's images
        /// </summary>
        /// <param name="productid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("{productid}/images")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddProductImages([FromRoute] long productid, List<FileBaseRequest> model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }
            
            return ResponseResult(await _productsManager.AddProductImages(productid, model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Set a default image
        /// </summary>
        /// <param name="productid"></param>
        /// <param name="imageid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("{productid}/images/{imageid}/set-default")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SetDefaultProductImage([FromRoute] long productid, [FromRoute] long imageid)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _productsManager.SetDefaultProductImage(productid, imageid, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Delete product's images
        /// </summary>
        /// <param name="imageids"></param>
        /// <param name="productid"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{productid}/images")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteProductImages([FromRoute] long productid, List<long> imageids)
        {
            if (imageids.Count() == 0 && productid == 0)
            {
                throw new Exception("Requested id is not valid.");
            }
           
            return ResponseResult(await _productsManager.DeleteProductImages(imageids, productid, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Move a product to another user's location
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("move-to-other-user")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> MoveProductsToOtherUserLocation(MoveProductsToAnotherUserLocationRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _productsManager.MoveProductsToAnotherUserLocation(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Cancel move request of product
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("cancel-move-request")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelProductMoveRequest(long productId)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _productsManager.CancelProductMoveRequest(productId, MovedStatus.Cancelled, User.Claims.GetUserId()));
        }
    }
}
