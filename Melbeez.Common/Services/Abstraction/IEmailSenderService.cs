using SendGrid;
using System.Threading.Tasks;

namespace Melbeez.Common.Services.Abstraction
{
    public interface IEmailSenderService
    {
        Task SendAsync(string to, string subject, string htmlMessage);
        Task<Response> SendMail(string to, string subject, string htmlMessag, string attachmentPath, string attachmentFileName);
        bool SendSMS(string MobileNo, string Message, long SMSTemplatesId);
    }
}
