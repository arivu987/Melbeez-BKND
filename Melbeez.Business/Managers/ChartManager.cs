using Melbeez.Business.Common.Services;
using Melbeez.Business.Managers.Abstractions;
using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using Melbeez.Common.Extensions;
using Melbeez.Common.Helpers;
using Melbeez.Data.UnitOfWork;
using Melbeez.Domain.Entities.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Melbeez.Business.Managers
{
    public class ChartManager : IChartManager
    {
        private readonly IWebHostEnvironment environment;
        private readonly IUnitOfWork unitOfWork;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUsersActivityTransactionLogManager usersActivityManager;
        public ChartManager(IUnitOfWork unitOfWork,
                            UserManager<ApplicationUser> userManager,
                            IUsersActivityTransactionLogManager usersActivityManager,
                            IWebHostEnvironment environment)
        {
            this.unitOfWork = unitOfWork;
            this.userManager = userManager;
            this.usersActivityManager = usersActivityManager;
            this.environment = environment;
        }
        public ManagerBaseResponse<DashboardChartResponseModel> Get(string userId)
        {
            var response = new ManagerBaseResponse<DashboardChartResponseModel>();
            var chart = new ChartService();
            var chartModel = new DashboardChartPrepareModel();

            AddUserActivity(userId);

            var products = unitOfWork
                .ProductsRepository
                .GetQueryable(x => x.CreatedBy == userId && !x.IsDeleted && x.Status != MovedStatus.Transferred)
                .Include(x => x.ProductCategoriesDetail)
                .Include(x => x.LocationsDetail)
                .ToList();

            var warrentiesCount = unitOfWork
                .ProductWarrantiesRepository
                .GetQueryable(x => !x.IsDeleted && x.IsProduct
                                && x.CreatedBy == userId
                                && x.EndDate.Date > DateTime.UtcNow.Date)
                .Include(x => x.ProductDetail)
                .Where(x => x.ProductDetail.Status != MovedStatus.Transferred)
                .ToList()
                .Count();

            #region Get product count by categories

            chartModel.ProductByCategories = products
                .Where(x => x.ProductCategoriesDetail != null)
                .Select(p => new
                {
                    Project = p,
                    CategoryId = p.CategoryId,
                    CategoryName = p.ProductCategoriesDetail?.Name
                })
                .GroupBy(u => new { u.CategoryId, u.CategoryName })
                .Select((g, i) => new
                {
                    Count = g.Where(x => x.CategoryId == g.Key.CategoryId).Count(),
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName
                })
                .Select(x => new ProductByCategoriesModel()
                {
                    CategoryName = x.CategoryName,
                    ProductByCategoriesCount = x.Count
                })
                .ToList();

            #endregion

            #region Get product count by location

            chartModel.ProductByLocations = products
                .Select(p => new
                {
                    Project = p,
                    LocationId = p.LocationId,
                    LocationName = p.LocationsDetail.Name
                })
                .GroupBy(u => new { u.LocationId, u.LocationName })
                .Select((g, i) => new
                {
                    Count = g.Where(x => x.LocationId == g.Key.LocationId).Count(),
                    LocationId = g.Key.LocationId,
                    LocationName = g.Key.LocationName
                })
                .Select(x => new ProductByLocationsModel()
                {
                    LocationName = x.LocationName,
                    ProductByLocationsCount = x.Count
                })
                .ToList();

            #endregion

            #region Get warrenty coverage

            var warrentyCoveragePercentage = products
                .Count() == 0 ? 0 : decimal.Parse(String.Format("{0:0.00}", (decimal)(((float)warrentiesCount / (float)products.Count()) * 100.00)));

            var warrentyCoverageModel = new WarrentyCoverageModel()
            {
                WarrentyCoveragePercentage = warrentyCoveragePercentage,
                WarrentyNotCoverPercentage = (100 - warrentyCoveragePercentage),
            };

            #endregion

            #region Get soon expiry warrenties

            DateTime currentDate = DateTime.UtcNow.Date;
            DateTime endDateThreshold = currentDate.AddDays(15);
            var warrantiesInfo = unitOfWork.ProductWarrantiesRepository
                .GetQueryable(x => !x.IsDeleted && x.IsProduct && x.CreatedBy == userId
                                && x.EndDate.Date >= currentDate && x.EndDate.Date <= endDateThreshold)
                .Include(x => x.ProductDetail)
                .Where(x => x.ProductDetail.Status != MovedStatus.Transferred)
                .Select(x => new WarrentyModel()
                {
                    WarrantyId = x.Id,
                    Name = x.Name,
                    ProductId = x.ProductId,
                    ProductName = x.ProductDetail.Name,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    ImageUrl = x.ImageUrl,
                })
                .OrderBy(x => x.EndDate)
                .Take(5)
                .ToListAsync();

            #endregion

            #region Get Asset Values

            chartModel.AssetValuesPerYear = products
                .Where(x => x.PurchaseDate != null)
                .Select(p => new
                {
                    Project = p,
                    Year = p.PurchaseDate.Value.Year,

                })
                .GroupBy(x => new { x.Year })
                .OrderBy(x => x.Key.Year)
                .Select((g, i) => new
                {
                    AssetValue = Convert.ToDouble(g.Where(x => x.Year == g.Key.Year)
                                                  .Select(x => x.Project.Price)
                                                  .ToList()
                                                  .Sum()
                                                  .ToString("#.00")
                                                 ),
                    Year = g.Key.Year
                })
                .Select(x => new AssetValuePerYearModel()
                {
                    AssetValue = x.AssetValue,
                    Year = x.Year.ToString()
                })
                .ToList();

            #endregion

            response.Result = new DashboardChartResponseModel()
            {
                WarrentyCoverage = warrentyCoverageModel,
                ExpireWarranties = warrantiesInfo.Result,
                ProductByCategoriesPieChart = chart.GeneratePieChart(chartModel, "pie", "ProductByCategories"),
                ProductByLocationsPieChart = chart.GeneratePieChart(chartModel, "pie", "ProductByLocations"),
                AssetValuesPerYearBarChart = chart.GenerateBarChart(chartModel, "bar")
            };
            return response;
        }
        public ManagerBaseResponse<AdminDashboardChartResponseModel> GetAdminDashboardChart(FilterGraphModel filterGraphModel)
        {
            var response = new ManagerBaseResponse<AdminDashboardChartResponseModel>();
            var chart = new ChartService();
            var chartModel = new AdminDashboardChartPrepareModel();

            #region Get Data for all chats by filter monthly and yearly

            var regUsers = userManager.Users.Where(x => !x.IsDeleted).ToList();
            var activeUserCount = 0;
            var inactiveUserCount = 0;
            foreach (var item in regUsers)
            {
                var loginHistory = unitOfWork
                              .UsersActivityTransactionLogRepository
                              .GetQueryable(x => !x.IsDeleted && x.CreatedBy == item.Id)
                              .Select(e => new
                              {
                                  UserId = e.CreatedBy,
                                  ActiveDate = e.CreatedOn
                              }).OrderByDescending(x => x.ActiveDate)
                              .FirstOrDefault();
                if (loginHistory != null)
                {
                    if (loginHistory.ActiveDate >= DateTime.UtcNow.AddDays(-30))
                        activeUserCount++;
                    else
                        inactiveUserCount++;
                }
            }

            var oTPsList = unitOfWork.OTPRepositry
                .GetQueryable()
                .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedOn.Month == filterGraphModel.Month && x.CreatedOn.Year == filterGraphModel.Year)
                .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedOn.Year == filterGraphModel.Year)
                .ToList();

            var usersList = userManager.Users
                .Where(x => !x.IsDeleted && x.IsUser)
                .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedDate.Month == filterGraphModel.Month && x.CreatedDate.Year == filterGraphModel.Year)
                .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedDate.Year == filterGraphModel.Year)
                .ToList();

            var queryUniqueActiveUsers = unitOfWork
                .UsersActivityTransactionLogRepository
                .GetQueryable(x => !x.IsDeleted && !x.applicationUser.IsDeleted && x.applicationUser.IsUser)
                .Include(x => x.applicationUser)
                .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedOn.Month == filterGraphModel.Month && x.CreatedOn.Year == filterGraphModel.Year)
                .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedOn.Year == filterGraphModel.Year);

            var barCodeAPILog = unitOfWork
                .BarCodeTransactionLogsRepository
                .GetQueryable(x => !x.IsDeleted)
                .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedOn.Month == filterGraphModel.Month && x.CreatedOn.Year == filterGraphModel.Year)
                .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedOn.Year == filterGraphModel.Year)
                .ToList();

            #endregion

            #region Process on data, Group data into weekly and monthly

            if (filterGraphModel.IsWeekly)
            {
                #region Get OTP Count Send On Email

                chartModel.EmailOTPsCountModel = oTPsList
                    .Where(x => x.Email != null)
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedOn.Year,
                        Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear
                                (p.CreatedOn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                    })
                    .GroupBy(x => new { x.Year, x.Week })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        WeekNum = i + 1,
                        Year = g.Key.Year,
                        CalendarWeek = g.Key.Week
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Week = x.WeekNum,
                        CalenderWeek = x.CalendarWeek,
                        Month = (Months)filterGraphModel.Month,
                        Year = x.Year,
                    })
                    .ToList();

                #endregion

                #region Get OTP Count Send On SMS

                chartModel.PhoneOTPsCountModel = oTPsList
                    .Where(x => x.PhoneNumber != null)
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedOn.Year,
                        Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear
                                (p.CreatedOn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                    })
                    .GroupBy(x => new { x.Year, x.Week })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        WeekNum = i + 1,
                        Year = g.Key.Year,
                        CalendarWeek = g.Key.Week
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Week = x.WeekNum,
                        CalenderWeek = x.CalendarWeek,
                        Month = (Months)filterGraphModel.Month,
                        Year = x.Year
                    })
                    .ToList();

                #endregion

                #region Get Newly Added Users Count

                chartModel.NewUsersCountModel = usersList
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedDate.Year,
                        Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(p.CreatedDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                    })
                    .GroupBy(x => new { x.Year, x.Week })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        WeekNum = i + 1,
                        Year = g.Key.Year,
                        CalendarWeek = g.Key.Week
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Week = x.WeekNum,
                        CalenderWeek = x.CalendarWeek,
                        Month = (Months)filterGraphModel.Month,
                        Year = x.Year
                    })
                    .ToList();

                #endregion

                #region Get Unique Active Users Count

                chartModel.UniqueActiveUsersCountModel = queryUniqueActiveUsers
                    .Select(x => new
                    {
                        x.CreatedBy,
                        x.CreatedOn.Year,
                        WeekNo = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(x.CreatedOn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),
                    })
                    .ToList()
                    .Distinct()
                    .GroupBy(x => new
                    {
                        x.Year,
                        x.WeekNo
                    })
                    .Select(x => new GeneralDataCountModel
                    {
                        Count = x.Count(),
                        Week = x.Key.WeekNo,
                        CalenderWeek = x.Key.WeekNo,
                        Year = x.Key.Year,
                    })
                    .ToList();

                #endregion

                #region Get BarCode API (Exernal API) Call Count

                chartModel.BarCodeAPICountModel = barCodeAPILog
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedOn.Year,
                        Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(p.CreatedOn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                    })
                    .GroupBy(x => new { x.Year, x.Week })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        WeekNum = i + 1,
                        Year = g.Key.Year,
                        CalendarWeek = g.Key.Week
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Week = x.WeekNum,
                        CalenderWeek = x.CalendarWeek,
                        Month = (Months)filterGraphModel.Month,
                        Year = x.Year
                    })
                    .ToList();

                #endregion

                switch (filterGraphModel.GraphName)
                {
                    case "EmailOTPCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            EmailOTPsCountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "EmailOTPsCountBar", (Months)filterGraphModel.Month, filterGraphModel.Year)
                        };
                        break;

                    case "PhoneOTPCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            PhoneOTPsCountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "PhoneOTPsCount", (Months)filterGraphModel.Month, filterGraphModel.Year)
                        };
                        break;

                    case "NewUsersCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            NewUsersCountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "NewUsersCountBar", (Months)filterGraphModel.Month, filterGraphModel.Year)
                        };
                        break;

                    case "ActiveUsersCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            UniqueActiveUsersBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "ActiveUsersCountBar", (Months)filterGraphModel.Month, filterGraphModel.Year)
                        };
                        break;

                    case "BarCodeAPICountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            BarCodeAPICountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "BarCodeAPICountBar", (Months)filterGraphModel.Month, filterGraphModel.Year)
                        };
                        break;

                    default:
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            EmailOTPsCountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "EmailOTPsCountBar", (Months)filterGraphModel.Month, filterGraphModel.Year),
                            PhoneOTPsCountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "PhoneOTPsCount", (Months)filterGraphModel.Month, filterGraphModel.Year),
                            NewUsersCountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "NewUsersCountBar", (Months)filterGraphModel.Month, filterGraphModel.Year),
                            UniqueActiveUsersBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "ActiveUsersCountBar", (Months)filterGraphModel.Month, filterGraphModel.Year),
                            BarCodeAPICountBarChart = chart.GenerateWeeklyBarChart(chartModel, "bar", "BarCodeAPICountBar", (Months)filterGraphModel.Month, filterGraphModel.Year)
                        };
                        break;
                }
            }
            else
            {
                #region Get OTP Count Send On Email

                chartModel.EmailOTPsCountModel = oTPsList
                    .Where(x => x.Email != null)
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedOn.Year,
                        Month = p.CreatedOn.Month
                    })
                    .GroupBy(x => new { x.Year, x.Month })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        Year = g.Key.Year,
                        Month = g.Key.Month
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Month = (Months)x.Month,
                        Year = x.Year
                    })
                    .ToList();

                #endregion

                #region Get OTP Count Send On SMS

                chartModel.PhoneOTPsCountModel = oTPsList
                    .Where(x => x.PhoneNumber != null)
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedOn.Year,
                        Month = p.CreatedOn.Month
                    })
                    .GroupBy(x => new { x.Year, x.Month })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        Year = g.Key.Year,
                        Month = g.Key.Month
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Month = (Months)x.Month,
                        Year = x.Year
                    })
                    .ToList();

                #endregion

                #region Get Newly Added Users Count

                chartModel.NewUsersCountModel = usersList
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedDate.Year,
                        Month = p.CreatedDate.Month
                    })
                    .GroupBy(x => new { x.Year, x.Month })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        Year = g.Key.Year,
                        Month = g.Key.Month
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Month = (Months)x.Month,
                        Year = x.Year
                    })
                    .ToList();

                #endregion

                #region Get Unique Active Users Count

                chartModel.UniqueActiveUsersCountModel = queryUniqueActiveUsers
                    .Select(x => new
                    {
                        x.CreatedBy,
                        x.CreatedOn.Year,
                        x.CreatedOn.Month
                    })
                    .ToList()
                    .Distinct()
                    .GroupBy(x => new
                    {
                        x.Year,
                        x.Month
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count(),
                        Month = (Months)x.Key.Month,
                        Year = x.Key.Year
                    })
                    .ToList();

                #endregion

                #region Get BarCode API (Exernal API) Call Count

                chartModel.BarCodeAPICountModel = barCodeAPILog
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.CreatedOn.Year,
                        Month = p.CreatedOn.Month
                    })
                    .GroupBy(x => new { x.Year, x.Month })
                    .Select((g, i) => new
                    {
                        Count = g.Count(),
                        Year = g.Key.Year,
                        Month = g.Key.Month
                    })
                    .Select(x => new GeneralDataCountModel()
                    {
                        Count = x.Count,
                        Month = (Months)x.Month,
                        Year = x.Year
                    })
                    .ToList();

                #endregion

                switch (filterGraphModel.GraphName)
                {
                    case "EmailOTPCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            EmailOTPsCountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "EmailOTPsCountBar")
                        };
                        break;

                    case "PhoneOTPCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            PhoneOTPsCountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "PhoneOTPsCount")
                        };
                        break;

                    case "NewUsersCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            NewUsersCountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "NewUsersCountBar")
                        };
                        break;

                    case "ActiveUsersCountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            UniqueActiveUsersBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "ActiveUsersCountBar")
                        };
                        break;

                    case "BarCodeAPICountChart":
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            BarCodeAPICountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "BarCodeAPICountBar")
                        };
                        break;

                    default:
                        response.Result = new AdminDashboardChartResponseModel()
                        {
                            ActiveUsersCount = activeUserCount,
                            InActiveUsersCount = inactiveUserCount,
                            EmailOTPsCountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "EmailOTPsCountBar"),
                            PhoneOTPsCountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "PhoneOTPsCount"),
                            NewUsersCountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "NewUsersCountBar"),
                            UniqueActiveUsersBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "ActiveUsersCountBar"),
                            BarCodeAPICountBarChart = chart.GenerateMonthlyBarChart(chartModel, "bar", "BarCodeAPICountBar")
                        };
                        break;
                }
            }

            #endregion

            return response;
        }
        public ManagerBaseResponse<string> ExportToExcel(FilterGraphModel filterGraphModel)
        {
            var response = new ManagerBaseResponse<string>();
            DataTable dt1 = new DataTable(filterGraphModel.GraphName);

            if (filterGraphModel.GraphName == "EmailOTPCountChart" || filterGraphModel.GraphName == "PhoneOTPCountChart")
            {
                var oTPsList = unitOfWork
                    .OTPRepositry
                    .GetQueryable()
                    .Include(x => x.applicationUser)
                    .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedOn.Month == filterGraphModel.Month && x.CreatedOn.Year == filterGraphModel.Year)
                    .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedOn.Year == filterGraphModel.Year)
                    .ToList();

                if (filterGraphModel.GraphName == "EmailOTPCountChart")
                {
                    var otpListData = oTPsList
                        .Where(x => x.Email != null)
                        .Select(x => new OTPOnEmailReportModel()
                        {
                            Name = string.Concat(x.applicationUser.FirstName, " ", x.applicationUser.LastName),
                            Email = x.Email,
                            OTP = x.OTP,
                            TimeStamp = x.CreatedOn,
                        })
                        .ToList();

                    dt1 = UtilityHelper.ToDataTable(otpListData);
                }
                else
                {
                    var otpListData = oTPsList
                        .Where(x => x.PhoneNumber != null)
                        .Select(x => new OTPOnSMSReportModel()
                        {
                            Name = string.Concat(x.applicationUser.FirstName, " ", x.applicationUser.LastName),
                            PhoneNumber = x.PhoneNumber,
                            OTP = x.OTP,
                            TimeStamp = x.CreatedOn,
                        })
                        .ToList();

                    dt1 = UtilityHelper.ToDataTable(otpListData);
                }
            }
            if (filterGraphModel.GraphName == "NewUsersCountChart")
            {
                var newUsersList = userManager.Users
                    .Where(x => !x.IsDeleted)
                    .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedDate.Month == filterGraphModel.Month && x.CreatedDate.Year == filterGraphModel.Year)
                    .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedDate.Year == filterGraphModel.Year)
                    .Select(x => new NewUsersReportModel()
                    {
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        Email = x.Email,
                        IsEmailConfimed = x.EmailConfirmed,
                        PhoneNumber = x.PhoneNumber,
                        IsPhoneNumberConfimed = x.PhoneNumberConfirmed,
                        RegisteredDate = x.CreatedDate
                    })
                    .ToList();

                dt1 = UtilityHelper.ToDataTable(newUsersList);
            }
            if (filterGraphModel.GraphName == "ActiveUsersCountChart")
            {
                var queryUniqueActiveUsers = unitOfWork
                    .UsersActivityTransactionLogRepository
                    .GetQueryable(x => !x.IsDeleted && !x.applicationUser.IsDeleted)
                    .Include(x => x.applicationUser)
                    .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedOn.Month == filterGraphModel.Month && x.CreatedOn.Year == filterGraphModel.Year)
                    .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedOn.Year == filterGraphModel.Year)
                    .ToList();

                if (filterGraphModel.IsWeekly)
                {
                    var uniqueActiveWeeklyData = queryUniqueActiveUsers
                        .GroupBy(x => new
                        {
                            FirstName = x.applicationUser.FirstName,
                            LastName = x.applicationUser.LastName,
                            Email = x.applicationUser.Email,
                            PhoneNumber = x.applicationUser.PhoneNumber,
                            Year = x.CreatedOn.Year,
                            WeekNo = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(x.CreatedOn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                        })
                        .Select(x => new ActiveUsersReportModel()
                        {
                            FirstName = x.Key.FirstName,
                            LastName = x.Key.LastName,
                            Email = x.Key.Email,
                            PhoneNumber = x.Key.PhoneNumber,
                            ActiveDate = x.Max(y => y.CreatedOn)
                        })
                        .Select(x => new ActiveUsersReportModel()
                        {
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            Email = x.Email,
                            PhoneNumber = x.PhoneNumber,
                            ActiveDate = x.ActiveDate,
                            Year = x.ActiveDate.Year,
                            Month = !filterGraphModel.IsWeekly ? Convert.ToString((Months)x.ActiveDate.Month) : null,
                            Week = filterGraphModel.IsWeekly ? Convert.ToString((Months)x.ActiveDate.Month) + " " + x.ActiveDate.Year.ToString().Substring(2, 2) + " Week " +
                                   CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(x.ActiveDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                                   : null
                        })
                        .ToList();

                    dt1 = UtilityHelper.ToDataTable(uniqueActiveWeeklyData);
                }
                else
                {
                    var uniqueActiveMonthlyData = queryUniqueActiveUsers
                        .GroupBy(x => new
                        {
                            FirstName = x.applicationUser.FirstName,
                            LastName = x.applicationUser.LastName,
                            Email = x.applicationUser.Email,
                            PhoneNumber = x.applicationUser.PhoneNumber,
                            Year = x.CreatedOn.Year,
                            Month = x.CreatedOn.Month
                        })
                        .Select(x => new ActiveUsersReportModel()
                        {
                            FirstName = x.Key.FirstName,
                            LastName = x.Key.LastName,
                            Email = x.Key.Email,
                            PhoneNumber = x.Key.PhoneNumber,
                            ActiveDate = x.Max(y => y.CreatedOn)
                        })
                        .Select(x => new ActiveUsersReportModel()
                        {
                            FirstName = x.FirstName,
                            LastName = x.LastName,
                            Email = x.Email,
                            PhoneNumber = x.PhoneNumber,
                            ActiveDate = x.ActiveDate,
                            Year = x.ActiveDate.Year,
                            Month = !filterGraphModel.IsWeekly ? Convert.ToString((Months)x.ActiveDate.Month) : null,
                            Week = filterGraphModel.IsWeekly ? Convert.ToString((Months)x.ActiveDate.Month) + " " + x.ActiveDate.Year.ToString().Substring(2, 2) + " Week " +
                                   CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(x.ActiveDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
                                   : null
                        })
                        .ToList();

                    dt1 = UtilityHelper.ToDataTable(uniqueActiveMonthlyData);
                }
            }
            if (filterGraphModel.GraphName == "BarCodeAPICountChart")
            {
                var barCodeAPILog = unitOfWork
                    .BarCodeTransactionLogsRepository
                    .GetQueryable(x => !x.IsDeleted)
                    .WhereIf(filterGraphModel.IsWeekly, x => x.CreatedOn.Month == filterGraphModel.Month && x.CreatedOn.Year == filterGraphModel.Year)
                    .WhereIf(!filterGraphModel.IsWeekly, x => x.CreatedOn.Year == filterGraphModel.Year)
                    .Select(x => new BarCodeAPIReportModel()
                    {
                        BarCode = x.BarCode,
                        Title = x.Title,
                        Status = x.Status,
                        ErrorMessage = x.ErrorMessage,
                        ScanBy = string.Concat(userManager.Users.FirstOrDefault(u => !u.IsDeleted && u.Id == x.CreatedBy).FirstName
                                               , " ", userManager.Users.FirstOrDefault(u => !u.IsDeleted && u.Id == x.CreatedBy).LastName),
                        ScanOn = x.CreatedOn
                    })
                    .ToList();
                dt1 = UtilityHelper.ToDataTable(barCodeAPILog);
            }
            byte[] file;
            string filePath = Path.Combine(environment.WebRootPath, "Reports/Report.xlsx");
            file = ExcelService.CreateExcelDocumentAsStream(dt1, filePath, 0, "Sheet1");
            response.Result = Convert.ToBase64String(file);
            return response;
        }
        public async void AddUserActivity(string userId)
        {
            DateTime startDateTime = DateTime.Today; //Today at 00:00:00
            DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59

            var todaysCount = unitOfWork
                .UsersActivityTransactionLogRepository
                .GetQueryable()
                .Where(x => !x.IsDeleted && x.CreatedOn >= startDateTime.ToUniversalTime()
                            && x.CreatedOn <= endDateTime.ToUniversalTime() && x.CreatedBy == userId)
                .ToList()
                .Count();

            if (todaysCount == 0)
            {
                var userActivityInfo = new UsersActivityTransactionLogResponseModel()
                {
                    UserId = userId,
                    IPAddress = UtilityHelper.GetLocalIPAddress(),
                    ActiveDate = DateTime.UtcNow
                };
                await usersActivityManager.AddTransactionLog(userActivityInfo);
            }
        }
    }
}
