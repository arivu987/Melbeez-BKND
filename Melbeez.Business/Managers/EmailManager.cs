using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Common.Services.Abstraction;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Melbeez.Business.Models.UserModels.ResponseModels;

namespace Melbeez.Business.Managers
{
    public class EmailManager : IEmailManager
    {
        private readonly IWebHostEnvironment environment;
        private readonly IEmailSenderService emailSenderService;
        private readonly IEmailTransactionLogManager emailTransactionLogManager;
        public EmailManager(
            IWebHostEnvironment environment,
            IEmailSenderService emailSenderService,
            IEmailTransactionLogManager emailTransactionLogManager
            )
        {
            this.environment = environment;
            this.emailSenderService = emailSenderService;
            this.emailTransactionLogManager = emailTransactionLogManager;
        }

        public async Task<ManagerBaseResponse<bool>> SetResetPasswordLinkEmail(string name, string userEmail, string link, string userId)
        {
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/Reset_Password.html"));
            htmlContent = htmlContent.Replace("{Name}", name);
            htmlContent = htmlContent.Replace("{ResetPasswordLink}", link);

            var response = SendAndManageMailLogs(userEmail, "Melbeez: Password Reset", htmlContent, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetRecoverUserNameEmail(string name, string userEmail, string userName, string userId)
        {
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/Recover_UserName.html"));
            htmlContent = htmlContent.Replace("{Name}", name);
            htmlContent = htmlContent.Replace("{UserName}", userName);

            var response = SendAndManageMailLogs(userEmail, "Melbeez: Forgot Username Recovery", htmlContent, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetOtpEmail(string name, string userEmail, string otp, string userId)
        {
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/OTP.html"));
            htmlContent = htmlContent.Replace("{Name}", name);
            htmlContent = htmlContent.Replace("{OTPCode}", otp);

            var response = await SendAndManageMailLogs(userEmail, "Melbeez: Password Reset", htmlContent, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result,
                Message = response.Message,
                StatusCode = response.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetEmailVerificationLink(string name, string userEmail, string emailVerificationUrl, string userId)
        {
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/Email_Verification.html"));
            htmlContent = htmlContent.Replace("{Name}", name);
            htmlContent = htmlContent.Replace("{ConfirmationLink}", emailVerificationUrl);

            var response = await SendAndManageMailLogs(userEmail, "Melbeez: Verify your email address", htmlContent, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result,
                Message = response.Message,
                StatusCode = response.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetEmailUpdateEmail(string userEmail, string name, string emailVerificationUrl, string userId)
        {
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/Profile_Email_Update.html"));
            htmlContent = htmlContent.Replace("{Name}", name);
            htmlContent = htmlContent.Replace("{ConfirmationLink}", emailVerificationUrl);

            var response = SendAndManageMailLogs(userEmail, "Melbeez: Verify your email address", htmlContent, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetItemTransferInvitationEmail(string userEmail, string name, string TransferItemName, string userId, bool isProduct)
        {
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/ItemTransferInvitation.html"));
            htmlContent = htmlContent.Replace("{Name}", name);
            htmlContent = htmlContent.Replace("{Item}", isProduct ? "Product" : "Location");
            htmlContent = htmlContent.Replace("{ItemName}", TransferItemName);

            var response = SendAndManageMailLogs(userEmail, "Melbeez: You have Invitation for Melbeez", htmlContent, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        public async Task<ManagerBaseResponse<bool>> SetItemTransferVerificationEmail(string name, string userEmail, string otp, string userId)
        {
            var htmlContent = File.ReadAllText(Path.Combine(environment.WebRootPath, "MailTemplates/ItemTransferVerification.html"));
            htmlContent = htmlContent.Replace("{Name}", name);
            htmlContent = htmlContent.Replace("{OTPCode}", otp);

            var response = SendAndManageMailLogs(userEmail, "Melbeez: User Verification", htmlContent, userId);
            return new ManagerBaseResponse<bool>()
            {
                Result = response.Result.Result,
                Message = response.Result.Message,
                StatusCode = response.Result.StatusCode
            };
        }
        private async Task<ManagerBaseResponse<bool>> SendAndManageMailLogs(string userEmail, string mailSubject, string mailBody, string userId)
        {
            var response = await emailSenderService.SendMail(userEmail, mailSubject, mailBody, null, null);
            await emailTransactionLogManager.AddEmailTransactionLog(new EmailTransactionLogResponseModel()
            {
                To = userEmail,
                Subject = mailSubject,
                Body = mailBody,
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                Status = response.StatusCode.ToString(),
                ErrorBody = response.Body.ReadAsStringAsync().Result
            }, userId);
            if (response.IsSuccessStatusCode)
            {
                return new ManagerBaseResponse<bool>()
                {
                    Result = response.IsSuccessStatusCode,
                    Message = "Mail has been sent successfully.",
                    StatusCode = (int)response.StatusCode
                };
            }
            return new ManagerBaseResponse<bool>()
            {
                Result = response.IsSuccessStatusCode,
                Message = "Send Email failed.",
                StatusCode = (int)response.StatusCode
            };
        }
    }
}
