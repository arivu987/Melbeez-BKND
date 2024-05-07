using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Services.Abstraction;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers
{
    /// <summary>
    /// SMS Manager is responsible for sending all the sms
    /// </summary>
    public class SMSManager : ISMSManager
    {
        private readonly IWebHostEnvironment environment;
        private readonly ISMSSenderService smsSenderService;
        private readonly ISMSTransactionLogManager smsTransactionLogManager;
        public SMSManager(IWebHostEnvironment environment,
                          ISMSSenderService smsSenderService,
                          ISMSTransactionLogManager smsTransactionLogManager)
        {
            this.environment = environment;
            this.smsSenderService = smsSenderService;
            this.smsTransactionLogManager = smsTransactionLogManager;
        }

        public async Task<ManagerBaseResponse<bool>> SetRecoverUsernameSms(string phoneNumber, string userName, string userId)
        {
            var bodySMS = "Per your request we are sharing your Melbeez username: {#username#}.\nIf you have not initiated this request please ignore this message.";
            bodySMS = bodySMS.Replace("{#username#}", userName);
            var response = SendAndManageSmsLogs(phoneNumber, bodySMS, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetOTPSms(string phoneNumber, string otp, string userId)
        {
            var bodySMS = "Please use OTP {#code#} to reset your Melbeez account password.\nIf you have not initiated a password change request please ignore this message.";
            bodySMS = bodySMS.Replace("{#code#}", otp);
            var response = SendAndManageSmsLogs(phoneNumber, bodySMS, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetPhoneVerificationLink(string phoneNumber, string phoneVerificationUrl, string userId)
        {
            var bodySMS = "Thank you for signing up with us.\nTo finish setting up your Melbeez account please verify your Mobile number by clicking on the link below:\n{#phoneverificationlink#}";
            bodySMS = bodySMS.Replace("{#phoneverificationlink#}", phoneVerificationUrl);
            var response = SendAndManageSmsLogs(phoneNumber, bodySMS, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetItemTransferInvitation(string phoneNumber, string invitationFromUserName, string userId)
        {
            var bodySMS = $"Hi,\nYou have an invitation from {invitationFromUserName} to join the Melbeez community.\nPlease join our app using links below:\nGoogle Plays Store: https://play.google.com/store/apps/details?id=com.melbeez.app&pli=1\nIOS App Store: https://apps.apple.com/za/app/melbeez/id6449959836";
            var response = SendAndManageSmsLogs(phoneNumber, bodySMS, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetItemTransferVerificationSms(string phoneNumber, string otp, string userId)
        {
            var bodySMS = "Please use OTP {#code#} to confirm user verification of your Melbeez account.\nIf you have not initiated a password change request please ignore this message.";
            bodySMS = bodySMS.Replace("{#code#}", otp);
            var response = SendAndManageSmsLogs(phoneNumber, bodySMS, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        private async Task<ManagerBaseResponse<bool>> SendAndManageSmsLogs(string phoneNumber, string message, string userId)
        {
            var response = await smsSenderService.SendSMS(phoneNumber, message);
            SendSmsResponse smsResponse = JsonConvert.DeserializeObject<SendSmsResponse>(response?.Content.ReadAsStringAsync().Result);
            await smsTransactionLogManager.AddSMSTransactionLog(new SMSTransactionLogResponseModel()
            {
                To = phoneNumber,
                Body = message,
                IsSuccess = response.IsSuccessStatusCode,
                SId = smsResponse.SId,
                StatusCode = (int)response.StatusCode,
                Status = response.StatusCode.ToString(),
                ErrorMessage = smsResponse.Message
            }, userId);

            if (response.IsSuccessStatusCode)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Result = response.IsSuccessStatusCode,
                    Message = "SMS has been sent successfully.",
                    StatusCode = (int)response.StatusCode
                };
            }
            return new ManagerBaseResponse<bool>()
            {
                Result = response.IsSuccessStatusCode,
                Message = smsResponse.Message.Contains("The 'To' number") ? smsResponse.Message.Substring(16) : smsResponse.Message,
                StatusCode = (int)response.StatusCode
            };
        }
    }
}
