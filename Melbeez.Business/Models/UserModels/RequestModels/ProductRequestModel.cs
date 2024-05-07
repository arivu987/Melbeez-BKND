using Melbeez.Common.Models.Common;
using Melbeez.Common.Models.Entities;
using System;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ProductRequestModel
    {
        public string? ProductName { get; set; }
        public string? ProductTitle { get; set; }
        public long LocationId { get; set; }
        public string ModelNumber { get; set; }
        public string ManufacturerName { get; set; }
        public string? SerialNumber { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string? VendorName { get; set; }
        public string Currency { get; set; }
        public double Price { get; set; }
        public long? CategoryId { get; set; }
        public List<FormFieldRequestModel> FormFields { get; set; }
        public List<FileBaseRequest> Files { get; set; }
        public List<ProductWarrantiesRequestModel> productWarrantiesRequestModels { get; set; }
        public long ReceiptId { get; set; }
        public string BarCodeNumber { get; set; }
        public string BarCodeData { get; set; }
        public string? Notes { get; set; }
        public string? OtherInfo { get; set; }
        public bool IsProductModelInfo { get; set; }
        public long? ProductModelInfoId { get; set; }
    }
}
