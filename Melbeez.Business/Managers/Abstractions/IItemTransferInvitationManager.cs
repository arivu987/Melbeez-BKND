using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.RequestModels;
using Melbeez.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IItemTransferInvitationManager
    {
        Task<ManagerBaseResponse<bool>> InviteUser(ItemTransferInvitationRequestModel model, string userId);
        Task InviteUserStatusUpdate(ApplicationUser user);
    }
}
