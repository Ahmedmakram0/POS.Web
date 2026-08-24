using POS.Web.ViewModels;

namespace POS.Web.Services.Reporting;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
