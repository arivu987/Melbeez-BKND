using Melbeez.Common.Helpers;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class MoveItemsResponse
    {
        public long Id { get; set; }
        public long LocationId { get; set; }
        public long ProductId { get; set; }
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public MovedStatus Status { get; set; }
    }
}
