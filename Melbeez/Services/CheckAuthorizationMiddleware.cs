using Melbeez.Business.Models.Common;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Melbeez.Services
{
    public class CheckAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        public CheckAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext httpContext, UserManager<ApplicationUser> _userManager)
        {
            string UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (UserId == null)
            { 
                await _next(httpContext);
            }
            else
            {
                var userData = _userManager.Users.Where(x => x.Id == UserId).FirstOrDefault();
                if (userData != null && userData.IsDeleted)
                {
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                    var responseObject = new ApiMiddlewareResponse()
                    {
                        isSuccess = false,
                        message = "User account does not exist",
                        statusCode = (int)HttpStatusCode.Unauthorized
                    };
                    var exceptionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObject));
                    await httpContext.Response.Body.WriteAsync(exceptionData, CancellationToken.None);
                }
                else
                {
                    await _next(httpContext);
                }
            }
        }
    }
}
