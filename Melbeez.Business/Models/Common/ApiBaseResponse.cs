using System;

namespace Melbeez.Business.Models.Common
{
    public class ApiBaseResponse<T>
    {
        public string Message { get; set; }
        public T Result { get; set; }
    }
    public class ApiBasePageResponse<T>
    {
        public string Message { get; set; }
        public T Result { get; set; }
        public PageDetailModel PageDetail { get; set; }
        public NotificationPageDetailModel NotificationPageDetail { get; set; }
    }
    public class ApiBaseFailResponse<T>
    {
        public string Message { get; set; }
        public T Result { get; set; }
    }
    public class ApiBaseFailResponse
    {
        public string Message { get; set; }
        public Guid TrackId { get; set; }
    }
}
