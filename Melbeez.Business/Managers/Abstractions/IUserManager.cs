using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Domain.Entities.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IUserManager
    {
        Task<CreateUserResponseModel> CreateUser(ApplicationUser user);
        Task<CreateUserResponseModel> CreateUser(ApplicationUser user, string password);
        Task<CreateUserResponseModel> CreateExternalProviderUser(ApplicationUser user, string provider, string providerId);
        ManagerBaseResponse<IEnumerable<Models.AccountModels.RoleModel>> GetRoleList();
        Task<ManagerBaseResponse<bool>> UpdateUser(string Id, ApplicationUserUpdateModel model);
        Task<ManagerBaseResponse<bool>> UpdateUserByEmail(string emailId, ApplicationUserUpdateModel model);
    }
}
