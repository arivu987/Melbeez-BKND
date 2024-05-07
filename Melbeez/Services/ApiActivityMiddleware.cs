using Melbeez.Business.Managers.Abstractions;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Melbeez.Services
{
    public class ApiActivityMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiActivityMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IAPIActivitiesManager _manager)
        {
            var watcher = Stopwatch.StartNew();
            string path = httpContext.Request.Path;
            string UserId = httpContext.User.Identity.Name;

            await _next(httpContext); 
            watcher.Stop();

            await _manager.Add(new Business.Models.APIActivitiesRequestModel()
            {
                APIPath = path,
                ExecutionTime = (int)watcher.ElapsedMilliseconds
            }, UserId);
        }
    }
}
