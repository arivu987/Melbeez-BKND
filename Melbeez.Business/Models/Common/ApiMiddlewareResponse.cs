namespace Melbeez.Business.Models.Common
{
    public class ApiMiddlewareResponse
    {
        public string message { get; set; }
        public bool isSuccess { get; set; }
        public int statusCode { get; set; }
    }
}
