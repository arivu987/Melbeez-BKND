using Melbeez.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ProductsResponseModel
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductTitle { get; set; }
        public string ModelNumber { get; set; }
        public string ManufacturerName { get; set; }
        public long LocationId { get; set; }
        public string Location { get; set; }
        public long? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string SerialNumber { get; set; }
        public string VendorName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Currency { get; set; }
        public double Price { get; set; }
        public string DefaultImageUrl { get; set; }
        public DateTime? WarrantyFrom { get; set; }
        public DateTime? WarrantyTo { get; set; }
        public string BarCodeNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsMoving { get; set; }
        public MovedStatus Status { get; set; }
        public string Notes { get; set; }
        public string OtherInfo { get; set; }
        public string? TransferId { get; set; }
        public string? TransferTo { get; set; }
        public DateTime? TransferExpireOn { get; set; }
        public DateTime? TransferInitiatedOn { get; set; }
    }
    public class ProductFormDataRequestModel : ProductsResponseModel
    {
        public string ProductFormData { get; set; }
        public IEnumerable<ProductImageResponseModel> ProductImages { get; set; }
        public long? ReceiptId { get; set; }
        public bool HasWarranty { get; set; }
    }
}
