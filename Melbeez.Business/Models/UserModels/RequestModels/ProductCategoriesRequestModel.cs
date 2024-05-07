using Melbeez.Common.Models.Entities;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ProductCategoriesRequestModel
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<FormFieldRequestModel> FormBuilderData { get; set; }
        public int Sequence { get; set; }
        public bool IsActive { get; set; }
    }
    public class ProductCategoriesBaseModel
    {
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsActive { get; set; }
    }
}
