using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Melbeez.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class ProductModelInfoResponse
    {
        public long Id { get; set; }
        public string ModelNumber { get; set; }
        public string ManufacturerName { get; set; }
        public long? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? ProductName { get; set; }
        public string? CategoryType { get; set; }
        public string? AutomotiveType { get; set; }
        public string? TypeofLicence { get; set; }
        public string? ExpiryDate { get; set; }
        public string? SystemType { get; set; }
        public string? PhoneOS { get; set; }
        public string? Capacity { get; set; }
        public string? NumberOfDoors { get; set; }
        public string? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public string? ManufactureYear { get; set; }
        public string? NoiseLevel { get; set; }
        public string? ControlButtonPlacement { get; set; }
        public string? CookingPower { get; set; }
        public string? Description { get; set; }
        public string? FormBuilderData { get; set; }
        public string? OtherInfo { get; set; }
        public ProductModelStatus Status { get; set; }
        public bool IsDraft { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class ManufacturerModelInfoResponse
    {
        public string ManufacturerName { get; set; }
        public HashSet<string> ModelNumber { get; set; }
    }
}
