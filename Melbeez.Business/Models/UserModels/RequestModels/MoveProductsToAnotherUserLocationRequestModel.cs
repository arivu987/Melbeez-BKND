using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class MoveProductsToAnotherUserLocationRequestModel
    {
        public List<long> ProductIds { get; set; }
        public long LocationId { get; set; }
        public string ToUserId { get; set; }
    }
    public class AddMoveProductsRequestModel
    {
        public long ProductId { get; set; }
        public long LocationId { get; set; }
        public string ToUserId { get; set; }
    }
}
