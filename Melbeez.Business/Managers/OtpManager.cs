using Melbeez.Business.Managers.Abstractions;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    public class OtpManager : IOtpManager
    {
        private readonly IUnitOfWork unitOfWork;
        public OtpManager(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task<string> AddOtp(string mobile, string email, string userId)
        {
            var expiryLimit = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(30);
            var currentTime = DateTime.UtcNow;
            var expiryThreshold = currentTime - expiryLimit;
            var otpResponse = unitOfWork.OTPRepositry
                    .GetQueryable(x => !x.IsDeleted && !x.IsUsed && x.UserId == userId && x.CreatedOn < expiryThreshold)
                    .ToList();
            foreach (var otp in otpResponse)
            {
                otp.IsUsed = true;
                otp.UpdatedOn = DateTime.UtcNow;
                otp.UpdatedBy = userId;
                await unitOfWork.CommitAsync();
            }

            Random random = new Random();
            var otpCode = random.Next(1000, 99999).ToString("D6");
            if (otpCode.Length != 6)
            {
                otpCode = random.Next(1000, 99999).ToString("D6");
            }

            var response = await unitOfWork.OTPRepositry.AddAsync(new OTPEntity()
            {
                UserId = userId,
                OTP = otpCode,
                PhoneNumber = mobile,
                Email = email,
                IsUsed = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
                IsDeleted = false
            });
            await unitOfWork.CommitAsync();

            return otpCode;
        }
        public async Task<bool> VerifyOtp(string otp, string OTPOverrideValue, string userId)
        {
            bool result = false;
            var expiryLimit = TimeSpan.FromMinutes(1) + TimeSpan.FromSeconds(30);
            var currentTime = DateTime.UtcNow;
            var expiryThreshold = currentTime - expiryLimit;
            var responseotp = unitOfWork.OTPRepositry
                        .GetQueryable(x => !x.IsDeleted && x.UserId == userId && x.IsUsed == false && x.CreatedOn >= expiryThreshold)
                        .FirstOrDefault();
            if (responseotp != null && responseotp.OTP == otp)
            {
                responseotp.IsUsed = true;
                responseotp.UpdatedOn = DateTime.UtcNow;
                responseotp.UpdatedBy = userId;
                await unitOfWork.CommitAsync();
                result = true;
            }
            else
            {
                if (otp == OTPOverrideValue) { result = true; }
            }
            return result;
        }
    }
}
