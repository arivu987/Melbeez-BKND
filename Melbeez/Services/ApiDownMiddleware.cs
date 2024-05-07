using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Melbeez.Services
{
    public class ApiDownMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiDownMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        private IAPIDownStatusManager _manager;
        private static bool? _isApiDown;
        bool GetApiDownStatus()
        {
            if (!_isApiDown.HasValue)
            {
                _isApiDown = _manager.GetLastApiStatusData().Result;
            }
            return (bool)_isApiDown;
        }
        public static void ResetApiDownStatus()
        {
            _isApiDown = null;
        }
        public async Task Invoke(HttpContext httpContext, IAPIDownStatusManager manager)
        {
            _manager = manager;
            string path = httpContext.Request.Path;

            if (httpContext.Request.Headers.TryGetValue("melbeez-platform", out var customPlateformHeader) && customPlateformHeader.ToString() == "AdminPortal")
            {
                await _next(httpContext);
                return;
            }
            if (!string.IsNullOrEmpty(path) && path.Contains("/api") && path != "/api/api-status" && httpContext.Request.Method != "OPTIONS")
            {
                if (GetApiDownStatus())
                {
                    httpContext.Response.ContentType = "application/json";
                    httpContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;

                    var responseObject = new ApiMiddlewareResponse()
                    {
                        isSuccess = false,
                        message = "System undergoing maintenance",
                        statusCode = (int)HttpStatusCode.ServiceUnavailable
                    };
                    var exceptionData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseObject));
                    await httpContext.Response.Body.WriteAsync(exceptionData, CancellationToken.None);
                }
                else
                {
                    await _next(httpContext);
                }
            }
            else
            {
                await _next(httpContext);
            }
        }
    }
}
