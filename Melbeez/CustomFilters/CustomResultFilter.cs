using Melbeez.Common.Models.Masters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Melbeez.CustomFilters
{
    public class CustomResultFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var obj = ((ObjectResult)context.Result);

            if (obj.GetType().Name == "BadRequestObjectResult")
            {
                var responseObj = obj.Value;
                if (responseObj.GetType().Name == "ManagerBaseResponse")
                {

                }


                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Result = new JsonResult(new MasterApiResponse<dynamic>()
                {
                    Result = obj.Value
                });
            }
            else
            {
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Result = new JsonResult(new MasterApiResponse<dynamic>()
                {
                    Result = obj.Value
                });
            }
        }
    }
}
