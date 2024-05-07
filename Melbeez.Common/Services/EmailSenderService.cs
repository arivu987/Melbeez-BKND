using Melbeez.Common.Services.Abstraction;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Melbeez.Common.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration configuration;

        public EmailSenderService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public Task SendAsync(string to, string subject, string htmlMessage)
        {
            var Host = configuration["EmailSender:Host"];
            var Port = Convert.ToInt32(configuration["EmailSender:Port"]);
            var EnableSSL = Convert.ToBoolean(configuration["EmailSender:EnableSSL"]);
            var UserName = configuration["EmailSender:UserName"];
            var Password = configuration["EmailSender:Password"];

            var client = new SmtpClient(Host, Port)
            {
                Credentials = new NetworkCredential(UserName, Password),
                EnableSsl = EnableSSL
            };
            return client.SendMailAsync(
                new MailMessage(UserName, to, subject, htmlMessage) { IsBodyHtml = true }
            );
        }
        public async Task<Response> SendMail(string to, string subject, string htmlMessage, string attachmentPath, string attachmentFileName)
        {
            try
            {
                var apiKey = configuration["SendGridConfiguration:ApiKey"];
                var UserName = configuration["SendGridConfiguration:UserName"];
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress(UserName);
                var msg = MailHelper.CreateSingleEmail(from, new EmailAddress(to), subject, null, htmlMessage);
                if (!string.IsNullOrEmpty(attachmentPath))
                {
                    byte[] bytes;
                    if (attachmentPath.Contains("http"))
                    {
                        using (HttpClient httpClient = new HttpClient())
                        {
                            HttpResponseMessage response = await httpClient.GetAsync(attachmentPath);
                            bytes = await response.Content.ReadAsByteArrayAsync();
                        }
                    }
                    else
                    {
                        bytes = System.IO.File.ReadAllBytes(attachmentPath);
                    }
                    var File = Convert.ToBase64String(bytes);
                    msg.AddAttachment(attachmentFileName, File);
                }
                return await client.SendEmailAsync(msg);
            }
            catch (Exception)
            {
                throw;
            }
           
        }
        public bool SendSMS(string MobileNo, string Message, long SMSTemplatesId)
        {
            try
            {
                string smsUrl = configuration["SMSSender:SMSURL"];
                smsUrl = smsUrl.Replace("{{Mobile}}", MobileNo);
                smsUrl = smsUrl.Replace("{{Message}}", Message);
                smsUrl = smsUrl.Replace("{{SMSTemplatesId}}", SMSTemplatesId.ToString());
                WebClient client = new WebClient();
                var content = client.DownloadString(smsUrl);
                if (content.Contains("100"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
