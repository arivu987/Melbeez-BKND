using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ItemTransferInvitationRequestModel
    {
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public List<long> TransferItemId { get; set; }
        public List<string> TransferItemName { get; set; }
        public bool IsProduct { get; set; }
    }
}
