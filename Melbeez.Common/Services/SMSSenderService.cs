using Melbeez.Common.Services.Abstraction;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Melbeez.Common.Services
{
    public class SMSSenderService : ISMSSenderService
    {
        private readonly IConfiguration configuration;

        public SMSSenderService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<HttpResponseMessage> SendSMS(string MobileNo, string Message)
        {
            try
            {
                // In production code, don't destroy the HttpClient through using, but better use IHttpClientFactory factory or at least reuse an existing HttpClient instance
                // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests
                // https://www.aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
                string smsUrl = configuration["SMSSender:SMSURL"];
                string authToken = configuration["SMSSender:AuthToken"];
                string from = configuration["SMSSender:From"];
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), smsUrl))
                    {
                        request.Headers.TryAddWithoutValidation("Authorization", authToken);

                        var contentList = new List<string>();
                        contentList.Add($"Body={Uri.EscapeDataString(Message)}");
                        contentList.Add($"From={Uri.EscapeDataString(from)}");
                        contentList.Add($"To={Uri.EscapeDataString(MobileNo)}");
                        request.Content = new StringContent(string.Join("&", contentList));
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                        var response = await httpClient.SendAsync(request);
                        return response;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
