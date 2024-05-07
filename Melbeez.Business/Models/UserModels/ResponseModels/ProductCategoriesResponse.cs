namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ProductCategoriesResponse
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
    }
    public class ProductCategoriesFormBuilderResponse : ProductCategoriesResponse
    {
        public string FormBuilderData { get; set; }
    }
}
