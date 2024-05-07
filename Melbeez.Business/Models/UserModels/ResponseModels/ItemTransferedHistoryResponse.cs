using Melbeez.Common.Helpers;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ItemTransferedHistoryResponse
    {
        public List<LocationTransferResponseModel> Locations { get; set; }
        public List<ProductsResponseModel> Products { get; set; }
        public bool IsProduct { get; set; }
    }
}
