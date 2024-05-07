using Melbeez.Business.Models;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Melbeez.Business.Common.Services
{
    public class ChartService
    {
        public GraphChartModel GeneratePieChart(DashboardChartPrepareModel model, string chartType, string chartFor)
        {
            var chartModel = new GraphChartModel();
            var colorData = new List<string> { "#0074D9", "#FF4136", "#2ECC40", "#FF851B", "#7FDBFF", "#B10DC9", "#FFDC00", "#001f3f", "#39CCCC", "#01FF70", "#85144b", "#F012BE", "#3D9970", "#111111", "#AAAAAA", "#790252", "#AF0171", "#3D8361", "#704F4F", "#224B0C" };
            var dataModel = new GraphDataModel();

            #region Dataset
            List<DatasetModel> datasets = new List<DatasetModel>();
            DatasetModel series = new DatasetModel();
            List<string> lables = new List<string>();
            series.backgroundColor = colorData;

            if (model.ProductByCategories.Count() > 0 && chartFor == "ProductByCategories")
            {
                foreach (var item in model.ProductByCategories)
                {
                    series.data.Add((decimal)item.ProductByCategoriesCount);
                    lables.Add(item.CategoryName);
                }
            }
            if (model.ProductByLocations.Count() > 0 && chartFor == "ProductByLocations")
            {
                foreach (var item in model.ProductByLocations)
                {
                    series.data.Add((decimal)item.ProductByLocationsCount);
                    lables.Add(item.LocationName);
                }
            }

            datasets.Add(series);
            dataModel.labels = lables;
            dataModel.datasets = datasets;
            #endregion

            chartModel.type = chartType;
            chartModel.data = dataModel;
            chartModel.options.scales = null;
            var font = new Fonts() { size = 20 };
            chartModel.options.plugins.legend.labels.fonts = font;
            chartModel.options.maintainAspectRatio = false;
            return chartModel;
        }
        public GraphChartModel GenerateBarChart(DashboardChartPrepareModel model, string chartType)
        {
            var chartModel = new GraphChartModel();
            var colorData = new List<string> { "#009688", "#ef5350", "#8e24aa" };
            var dataModel = new GraphDataModel();

            #region Dataset
            List<DatasetModel> datasets = new List<DatasetModel>();
            List<string> lables = new List<string>();

            if (model.AssetValuesPerYear != null)
            {
                foreach (var item in model.AssetValuesPerYear)
                {
                    lables.Add(item.Year);
                }
                datasets = BarChartDatasets(model.AssetValuesPerYear, colorData);
            }

            dataModel.labels = lables;
            dataModel.datasets = datasets;
            #endregion

            chartModel.type = chartType;
            chartModel.data = dataModel;
            chartModel.options.plugins.legend = null;
            chartModel.options.maintainAspectRatio = false;
            return chartModel;
        }
        public GraphChartModel GenerateMonthlyBarChart(AdminDashboardChartPrepareModel model, string chartType, string chartName)
        {
            var chartModel = new GraphChartModel();
            var colorData = new List<string> { "#009688", "#ef5350", "#8e24aa" };
            var dataModel = new GraphDataModel();

            #region Dataset
            List<string> lables = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            if (model.EmailOTPsCountModel != null && chartName == "EmailOTPsCountBar")
            {
                dataModel = MonthlyBarChartDatasets(model.EmailOTPsCountModel, colorData, lables);
            }
            if (model.PhoneOTPsCountModel != null && chartName == "PhoneOTPsCount")
            {
                dataModel = MonthlyBarChartDatasets(model.PhoneOTPsCountModel, colorData, lables);
            }
            if (model.NewUsersCountModel != null && chartName == "NewUsersCountBar")
            {
                dataModel = MonthlyBarChartDatasets(model.NewUsersCountModel, colorData, lables);
            }
            if (model.UniqueActiveUsersCountModel != null && chartName == "ActiveUsersCountBar")
            {
                dataModel = MonthlyBarChartDatasets(model.UniqueActiveUsersCountModel, colorData, lables);
            }
            if (model.BarCodeAPICountModel != null && chartName == "BarCodeAPICountBar")
            {
                dataModel = MonthlyBarChartDatasets(model.BarCodeAPICountModel, colorData, lables);
            }

            #endregion

            chartModel.type = chartType;
            chartModel.data = dataModel;
            chartModel.options.plugins.legend = null;
            chartModel.options.scales.x.ticks.stepSize = 50;
            chartModel.options.scales.y.ticks.stepSize = 50;
            chartModel.options.indexAxis = "y";
            chartModel.options.maintainAspectRatio = false;
            return chartModel;
        }
        public GraphChartModel GenerateWeeklyBarChart(AdminDashboardChartPrepareModel model, string chartType, string chartName, Months months, int? year)
        {
            var chartModel = new GraphChartModel();
            var colorData = new List<string> { "#009688", "#ef5350", "#8e24aa" };
            var dataModel = new GraphDataModel();

            #region Dataset

            if (model.EmailOTPsCountModel != null && chartName == "EmailOTPsCountBar")
            {
                dataModel = WeeklyBarChartDatasets(model.EmailOTPsCountModel, colorData, (Months)months, year);
            }
            if (model.PhoneOTPsCountModel != null && chartName == "PhoneOTPsCount")
            {
                dataModel = WeeklyBarChartDatasets(model.PhoneOTPsCountModel, colorData, (Months)months, year);
            }
            if (model.NewUsersCountModel != null && chartName == "NewUsersCountBar")
            {
                dataModel = WeeklyBarChartDatasets(model.NewUsersCountModel, colorData, (Months)months, year);
            }
            if (model.UniqueActiveUsersCountModel != null && chartName == "ActiveUsersCountBar")
            {
                dataModel = WeeklyBarChartDatasets(model.UniqueActiveUsersCountModel, colorData, (Months)months, year);
            }
            if (model.BarCodeAPICountModel != null && chartName == "BarCodeAPICountBar")
            {
                dataModel = WeeklyBarChartDatasets(model.BarCodeAPICountModel, colorData, (Months)months, year);
            }

            #endregion

            chartModel.type = chartType;
            chartModel.data = dataModel;
            chartModel.options.plugins.legend = null;
            chartModel.options.scales.x.ticks.stepSize = 50;
            chartModel.options.scales.y.ticks.stepSize = 50;
            chartModel.options.indexAxis = "y";
            chartModel.options.maintainAspectRatio = false;
            return chartModel;
        }
        private List<DatasetModel> BarChartDatasets(List<AssetValuePerYearModel> model, List<string> colorData)
        {
            List<DatasetModel> datasets = new List<DatasetModel>();
            DatasetModel series = new DatasetModel();
            foreach (var item in model)
            {
                series.data.Add((decimal)item.AssetValue);
            }

            var meBackGroupColor = new List<string>();
            var meBorderColor = new List<string>();
            foreach (var meItem in model)
            {
                meBackGroupColor.Add(colorData[0]);
            }

            meBorderColor.Add(colorData[0]);
            series.backgroundColor = meBackGroupColor;
            series.fill = false;
            datasets.Add(series);
            return datasets;
        }
        private GraphDataModel MonthlyBarChartDatasets(List<GeneralDataCountModel> model, List<string> colorData, List<string> months)
        {
            List<DatasetModel> datasets = new List<DatasetModel>();
            DatasetModel series = new DatasetModel();
            var dataModel = new GraphDataModel();
            List<string> lables = new List<string>();

            foreach (var item in months)
            {
                var monthData = model.Where(x => x.Month.ToString() == item).FirstOrDefault();
                if (monthData != null)
                {
                    lables.Add(item);
                    series.data.Add((decimal)monthData.Count);
                }
                else
                {
                    lables.Add(item);
                    series.data.Add(0);
                }
            }

            var meBackGroupColor = new List<string>();
            var meBorderColor = new List<string>();
            foreach (var meItem in model)
            {
                meBackGroupColor.Add(colorData[0]);
            }

            meBorderColor.Add(colorData[0]);
            series.backgroundColor = meBackGroupColor;
            series.fill = false;
            datasets.Add(series);

            dataModel.labels = lables;
            dataModel.datasets = datasets;

            return dataModel;
        }
        private GraphDataModel WeeklyBarChartDatasets(List<GeneralDataCountModel> model, List<string> colorData, Months months, int? year)
        {
            List<DatasetModel> datasets = new List<DatasetModel>();
            DatasetModel series = new DatasetModel();
            var dataModel = new GraphDataModel();
            List<string> lables = new List<string>();

            var weeksInMonth = GetCalenderWeekOfMonth((int)year, (Months)months).OrderBy(x => x).ToList();
            string label = (Months)months + " " + year.Value.ToString().Substring(2, 2) + " Week ";
            int weekCount = 1;

            foreach (var item in weeksInMonth)
            {
                var weekData = model.Where(x => x.CalenderWeek == item).FirstOrDefault();
                if (weekData != null)
                {
                    lables.Add(label + weekCount);
                    series.data.Add((decimal)weekData.Count);
                }
                else
                {
                    lables.Add(label + weekCount);
                    series.data.Add(0);
                }
                weekCount++;
            }

            var meBackGroupColor = new List<string>();
            var meBorderColor = new List<string>();
            foreach (var meItem in model)
            {
                meBackGroupColor.Add(colorData[0]);
            }

            meBorderColor.Add(colorData[0]);
            series.backgroundColor = meBackGroupColor;
            series.fill = false;
            datasets.Add(series);
            dataModel.labels = lables;
            dataModel.datasets = datasets;
            return dataModel;
        }
        private static List<int> GetCalenderWeekOfMonth(int year, Months month)
        {
            var firstDayOfMonth = new DateTime(year, (int)month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            List<int> result = new List<int>();

            for (int i = firstDayOfMonth.Day; i <= lastDayOfMonth.Day; i = i + 7)
            {
                var d = new DateTime(year, (int)month, i);
                CultureInfo cul = CultureInfo.CurrentCulture;
                int weekNum = cul.Calendar.GetWeekOfYear(d, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                result.Add(weekNum);
            }
            return result;
        }
    }
}
