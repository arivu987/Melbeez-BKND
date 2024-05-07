using Melbeez.Common.Helpers;
using System;

namespace Melbeez.Common.Models.Entities
{
    public class BaseWarrantiesResponseModel
    {
        public long ProductWarrantyId { get; set; }
        public string Name { get; set; }
        public long? CategoryId { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public MovedStatus ProductStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class ProductWarrantiesResponseModel : BaseWarrantiesResponseModel
    {
        public string Currency { get; set; }
        public double? Price { get; set; }
        public string Provider { get; set; }
        public string Type { get; set; }
        public string AgreementNumber { get; set; }
        public bool IsProduct { get; set; }
    }
}
