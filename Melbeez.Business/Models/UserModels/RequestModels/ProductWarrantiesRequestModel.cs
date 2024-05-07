using System;

namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ProductWarrantiesRequestModel
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Currency { get; set; }
        public double? Price { get; set; }
        public string Provider { get; set; }
        public string Type { get; set; }
        public string AgreementNumber { get; set; }
        public string ImageUrl { get; set; }
        public bool IsProduct { get; set; }
    }
}
