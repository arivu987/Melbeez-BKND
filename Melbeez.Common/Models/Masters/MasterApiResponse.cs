namespace Melbeez.Common.Models.Masters
{
    public class MasterApiResponse<T>
    {
        public string Message { get; set; }
        public T Result { get; set; }
        public MasterApiResponse()
        {
            Message = string.Empty;
        }
        public MasterApiResponse<T> FailResult(string ErrorMessage)
        {
            this.Message = ErrorMessage;
            return this;
        }
        public MasterApiResponse<T> UnAuthorizeResult(string message)
        {
            this.Message = message;
            return this;
        }
    }
}
