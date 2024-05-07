using Melbeez.Business.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Melbeez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        public IActionResult ResponseResult<T>(ManagerBaseResponse<T> response)
        {
            if (response.IsSuccess)
            {
                if (response.PageDetail == null)
                {
                    if (response.NotificationPageDetail != null)
                    {
                        return Ok(new ApiBasePageResponse<T>()
                        {

                            Message = response.Message,
                            Result = response.Result,
                            NotificationPageDetail = response.NotificationPageDetail
                        });

                    }
                    return Ok(new ApiBaseResponse<T>()
                    {

                        Message = response.Message,
                        Result = response.Result,
                    });
                }

                return Ok(new ApiBasePageResponse<T>()
                {

                    Message = response.Message,
                    Result = response.Result,
                    PageDetail = response.PageDetail
                });
            }
            else if (response.StatusCode == 401)
            {
                return Unauthorized(new ApiBaseFailResponse<T>()
                {

                    Message = response.Message,
                    Result = response.Result,
                });
            }
            else
            {
                return BadRequest(new ApiBaseFailResponse<T>()
                {

                    Message = response.Message,
                    Result = response.Result,
                });
            }
        }
        public IActionResult BadRequestResult<T>(ManagerBaseResponse<T> response)
        {
            return BadRequest(new ApiBaseFailResponse<T>()
            {

                Message = response.Message,
                Result = response.Result,
            });
        }
    }
}
