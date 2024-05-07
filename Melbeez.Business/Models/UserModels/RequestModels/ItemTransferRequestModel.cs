using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ItemTransferRequestModel
    {
        public string TransferTo { get; set; }
        public List<long> ItemIds { get; set; }
        public bool IsProduct { get; set; }
    }
}
