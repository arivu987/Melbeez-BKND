using Melbeez.Business.Models.Common;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface ISMSManager
    {
        Task<ManagerBaseResponse<bool>> SetRecoverUsernameSms(string phoneNumber, string userName, string userId);
        Task<ManagerBaseResponse<bool>> SetOTPSms(string phoneNumber, string otp, string userId);
        Task<ManagerBaseResponse<bool>> SetPhoneVerificationLink(string phoneNumber, string phoneVerificationUrl, string userId);
        Task<ManagerBaseResponse<bool>> SetItemTransferInvitation(string phoneNumber, string invitationFromUserName, string userId);
        Task<ManagerBaseResponse<bool>> SetItemTransferVerificationSms(string phoneNumber, string otp, string userId);
    }
}
