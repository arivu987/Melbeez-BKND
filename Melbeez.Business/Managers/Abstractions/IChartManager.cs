using Melbeez.Business.Models.Common;
using Melbeez.Business.Models.UserModels.ResponseModels;
using System.Threading.Tasks;

namespace Melbeez.Business.Managers.Abstractions
{
    public interface IChartManager
    {
        ManagerBaseResponse<DashboardChartResponseModel> Get(string userId);
        ManagerBaseResponse<AdminDashboardChartResponseModel> GetAdminDashboardChart(FilterGraphModel filterGraphModel);
        ManagerBaseResponse<string> ExportToExcel(FilterGraphModel filterGraphModel);
    }
}
