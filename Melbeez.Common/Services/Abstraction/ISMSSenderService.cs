using System.Net.Http;
using System.Threading.Tasks;

namespace Melbeez.Common.Services.Abstraction
{
    public interface ISMSSenderService
    {
        Task<HttpResponseMessage> SendSMS(string MobileNo, string Message);
    }
}
