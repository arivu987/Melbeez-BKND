using Melbeez.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class TransferItemResponse
    {
        public string TransferId { get; set; }  
        public List<ProductsResponseModel> ProductList { get; set; }
        public List<LocationTransferResponseModel> LocationList { get; set; }
        public bool IsProduct { get; set; }
        public string FromUserId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }
        public MovedStatus Status { get; set; }
        public DateTime? ExpireOn { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
