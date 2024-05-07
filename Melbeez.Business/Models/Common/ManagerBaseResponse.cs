namespace Melbeez.Business.Models.Common
{
    public class ManagerBaseResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public int StatusCode { get; set; } = 200;
        public T Result { get; set; }
        public PageDetailModel PageDetail { get; set; }
        public NotificationPageDetailModel NotificationPageDetail { get; set; }
        public ManagerBaseResponse()
        {
            IsSuccess = true;
        }
        public ManagerBaseResponse<T> Success(string message, T result)
        {
            IsSuccess = true;
            Message = message;
            Result = result;
            return this;
        }
        public ManagerBaseResponse<T> Success(string message, T result, PageDetailModel pageDetail, NotificationPageDetailModel notificationPageDetail)
        {
            IsSuccess = true;
            Message = message;
            Result = result;
            PageDetail = pageDetail;
            NotificationPageDetail = notificationPageDetail;
            return this;
        }
        public ManagerBaseResponse<T> Failed(int statusCode, string message)
        {
            IsSuccess = false;
            StatusCode = statusCode;
            Message = message;
            Result = default;
            return this;
        }
        public ManagerBaseResponse<T> Failed(int statusCode, string message, T result)
        {
            IsSuccess = false;
            StatusCode = statusCode;
            Message = message;
            Result = result;
            return this;
        }
    }
}
