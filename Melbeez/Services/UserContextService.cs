using System.Linq;
using System.Security.Claims;
using Melbeez.Common.Services.Abstraction;
using Microsoft.AspNetCore.Http;

namespace Melbeez.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public string GetUserId()
        {
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.User == null)
                return null;

            var claim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (claim == null)
                return null;

            return claim.Value;
        }
        public string GetUserName()
        {
            if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.User == null)
                return null;

            var claim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            if (claim == null)
                return null;

            return claim.Value;
        }
    }
}
