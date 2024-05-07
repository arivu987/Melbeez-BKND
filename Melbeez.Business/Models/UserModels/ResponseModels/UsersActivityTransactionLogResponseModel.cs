using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class UsersActivityTransactionLogResponseModel
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string IPAddress { get; set; }
        public DateTime ActiveDate { get; set; }
    }
}
