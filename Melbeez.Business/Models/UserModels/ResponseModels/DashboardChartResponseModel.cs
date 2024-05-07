using System;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class DashboardChartResponseModel
    {
        public WarrentyCoverageModel WarrentyCoverage { get; set; }
        public List<WarrentyModel> ExpireWarranties { get; set; }
        public GraphChartModel ProductByCategoriesPieChart { get; set; }
        public GraphChartModel ProductByLocationsPieChart { get; set; }
        public GraphChartModel AssetValuesPerYearBarChart { get; set; }
    }
    public class DashboardChartPrepareModel
    {
        public List<ProductByCategoriesModel> ProductByCategories { get; set; }
        public List<ProductByLocationsModel> ProductByLocations { get; set; }
        public List<AssetValuePerYearModel> AssetValuesPerYear { get; set; }
        public GraphChartModel ProductByCategoriesPieChart { get; set; }
        public GraphChartModel ProductByLocationsPieChart { get; set; }
    }
    public class WarrentyCoverageModel
    {
        public decimal? WarrentyCoveragePercentage { get; set; }
        public decimal? WarrentyNotCoverPercentage { get; set; }
    }
    public class WarrentyModel
    {
        public long WarrantyId { get; set; }
        public string Name { get; set; }
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class ProductByCategoriesModel
    {
        public string CategoryName { get; set; }
        public int? ProductByCategoriesCount { get; set; }
    }
    public class ProductByLocationsModel
    {
        public string LocationName { get; set; }
        public int? ProductByLocationsCount { get; set; }
    }
    public class AssetValuePerYearModel
    {
        public string Year { get; set; }
        public double? AssetValue { get; set; }
    }
}
