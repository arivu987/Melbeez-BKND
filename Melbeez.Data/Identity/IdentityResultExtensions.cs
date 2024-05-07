using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Melbeez.Common.Models;
using System.Collections.Generic;
using System.Linq;

namespace Melbeez.Data.Identity
{
    public static class IdentityResultExtensions
    {
        public static Result ToApplicationResult(this IdentityResult result)
        {
            return result.Succeeded
                ? Result.Success()
                : Result.Failure(result.Errors.Select(e => e.Description));
        }
        public static string GetUserId(this IEnumerable<Claim> claims)
        {
            var findUserId = claims.Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();
            if (findUserId != null && !string.IsNullOrWhiteSpace(findUserId.Value))
            {
                return findUserId.Value;
            }
            //TODO: handle this
            return "-1";
        }
        public static string GetUserRoles(this IEnumerable<Claim> claims)
        {
            var findroles = claims.Where(x => x.Type == ClaimTypes.Role).ToList();
            if (findroles != null && findroles.Count > 0)
            {
                return string.Join(",", findroles.Select(x => x.Value));
            }
            //TODO: handle this
            return "-1";
        }
    }
}
