using Melbeez.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class LocationsResponseModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string CityName { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string ZipCode { get; set; }
        public string Image { get; set; }
        public LocationType TypeOfProperty { get; set; }
        public bool IsDefault { get; set; }
        public long? CategoryId { get; set; }
        public int? ProductsCount { get; set; }
        public string Currency { get; set; }
        public double? TotalProductsAmount { get; set; }
        public DateTime? WarrantyFrom { get; set; }
        public DateTime? WarrantyTo { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public bool IsMoving { get; set; }
        public MovedStatus Status { get; set; }
        public string? TransferId { get; set; }
        public string? TransferTo { get; set; }
        public DateTime? TransferExpireOn { get; set; }
        public DateTime? TransferInitiatedOn { get; set; }
    }
    public class LocationTransferResponseModel : LocationsResponseModel
    {
       public List<ProductsTransferResponseModel> Products { get; set; }
    }
    public class ProductsTransferResponseModel
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductTitle { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Currency { get; set; }
        public double Price { get; set; }
        public string DefaultImageUrl { get; set; }
    }
}
