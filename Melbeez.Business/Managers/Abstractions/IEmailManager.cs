using Melbeez.Business.Models.Common;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IEmailManager
    {
        Task<ManagerBaseResponse<bool>> SetResetPasswordLinkEmail(string name, string userEmail, string link, string userId);
        Task<ManagerBaseResponse<bool>> SetRecoverUserNameEmail(string name, string userEmail, string userName, string userId);
        Task<ManagerBaseResponse<bool>> SetOtpEmail(string name, string userEmail, string otp, string userId);
        Task<ManagerBaseResponse<bool>> SetEmailVerificationLink(string name, string userEmail, string emailVerificationUrl, string userId);
        Task<ManagerBaseResponse<bool>> SetEmailUpdateEmail(string userEmail, string name, string emailVerificationUrl, string userId);
        Task<ManagerBaseResponse<bool>> SetItemTransferInvitationEmail(string userEmail, string name, string TransferItemName, string userId, bool isProduct);
        Task<ManagerBaseResponse<bool>> SetItemTransferVerificationEmail(string name, string userEmail, string otp, string userId);
    }
}
