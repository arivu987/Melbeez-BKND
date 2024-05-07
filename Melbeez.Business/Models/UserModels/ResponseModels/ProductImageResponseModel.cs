namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ProductImageResponseModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string ImageUrl { get; set; }
        public int FileSize { get; set; }
        public bool IsDefault { get; set; }
    }
}