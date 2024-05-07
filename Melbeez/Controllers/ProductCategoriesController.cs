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
    /// This endpoint contains CRUD operation of product's categories
    /// </summary>
    [Route("api/product-categories")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiBaseFailResponse), StatusCodes.Status500InternalServerError)]
    public class ProductCategoriesController : BaseController
    {
        private readonly IProductCategoriesManager _categoryManager;
        public ProductCategoriesController(IProductCategoriesManager categoryManager)
        {
            _categoryManager = categoryManager;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductCategoriesResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategory()
        {
            return ResponseResult(await _categoryManager.Get());
        }
        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("web")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductCategoriesResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryForWeb([FromQuery] PagedListCriteria pagedListCriteria)
        {
            return ResponseResult(await _categoryManager.GetCategoryForWeb(pagedListCriteria));
        }

        /// <summary>
        /// Get category by categoryId
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpGet("form-builder-data/{categoryId}")]
        [ProducesResponseType(typeof(ApiBasePageResponse<IEnumerable<ProductCategoriesFormBuilderResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFormBuilderByCategoryId(long categoryId)
        {
            try
            {
                return ResponseResult(await _categoryManager.GetFormBuilderByCategoryId(categoryId));
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
        /// Create a new category with common fields
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddCategory([FromBody]ProductCategoriesBaseModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _categoryManager.AddCategory(model, User.Claims.GetUserId()));
        }
        /// <summary>
        /// Update an existing category
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCategory([FromBody] ProductCategoriesBaseModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _categoryManager.UpdateCategory(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost("form-builder")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddCategoryWithFormBuilder([FromBody] ProductCategoriesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _categoryManager.AddCategoryWithFormBuilder(model, User.Claims.GetUserId()));
        }

        /// <summary>
        ///Update an existing category
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPut("update-with-form-builder")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateCategoryWithFormBuilder([FromBody] ProductCategoriesRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new Exception("Requested model is not valid.");
            }

            return ResponseResult(await _categoryManager.UpdateCategoryWithFormBuilder(model, User.Claims.GetUserId()));
        }

        /// <summary>
        /// Delete a category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiBaseFailResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteCategory([FromRoute] long id)
        {
            if (id == 0)
            {
                throw new Exception("Requested id is not valid.");
            }

            return ResponseResult(await _categoryManager.DeleteCategory(id, User.Claims.GetUserId()));
        }
    }
}
