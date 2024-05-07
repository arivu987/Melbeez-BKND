using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IOtpManager
    {
        Task<string> AddOtp(string mobile, string email, string userId);
        Task<bool> VerifyOtp(string otp, string OTPOverrideValue,string userId);
    }
}
