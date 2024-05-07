using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ContactusResponseModel
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
