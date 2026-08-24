using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS.Web.Models;
using POS.Web.Services.Reporting;

namespace POS.Web.Controllers;

[Authorize]
public class HomeController(IDashboardService dashboardService) : Controller
{
    public async Task<IActionResult> Index() => View(await dashboardService.GetDashboardAsync());

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
