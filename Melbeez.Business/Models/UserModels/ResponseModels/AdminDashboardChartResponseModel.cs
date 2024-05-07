using Melbeez.Common.Helpers;
using System;
using System.Collections.Generic;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class AdminDashboardChartResponseModel
    {
        public int ActiveUsersCount { get; set; }
        public int InActiveUsersCount { get; set; }
        public GraphChartModel EmailOTPsCountBarChart { get; set; }
        public GraphChartModel PhoneOTPsCountBarChart { get; set; }
        public GraphChartModel NewUsersCountBarChart { get; set; }
        public GraphChartModel UniqueActiveUsersBarChart { get; set; }
        public GraphChartModel BarCodeAPICountBarChart { get; set; }
    }
    public class AdminDashboardChartPrepareModel
    {
        public List<GeneralDataCountModel> EmailOTPsCountModel { get; set; }
        public List<GeneralDataCountModel> PhoneOTPsCountModel { get; set; }
        public List<GeneralDataCountModel> NewUsersCountModel { get; set; }
        public List<GeneralDataCountModel> UniqueActiveUsersCountModel { get; set; }
        public List<GeneralDataCountModel> BarCodeAPICountModel { get; set; }
    }
    public class GeneralDataCountModel
    {
        public int Count { get; set; }
        public int Week { get; set; }
        public int CalenderWeek { get; set; }
        public Months Month { get; set; }
        public int Year { get; set; }
    }
    public class FilterGraphModel
    {
        public string GraphName { get; set; }
        public int? Month { get; set; } = DateTime.UtcNow.Month;
        public int? Year { get; set; } = DateTime.UtcNow.Year;
        public bool IsWeekly { get; set; } = false;
    }
}
