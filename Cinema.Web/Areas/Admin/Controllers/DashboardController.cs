using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cinema.Web.Areas.Admin.ViewModels;
using Cinema.Application.Dashboard;

namespace Cinema.Web.Areas.Admin.Controllers;

[Area("Admin")]
// Admin area ana paneli. Admin ve SuperAdmin ayni dashboard'u kullanir.
[Authorize(Roles = "Admin,SuperAdmin")]
public class DashboardController : Controller
{
    private readonly IAdminDashboardService _dashboardService;

    public DashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    // Dashboard ana sayfasi (kisa yollar + ozet sayilar).
    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        var vm = new AdminDashboardVm
        {
            MovieCount = summary.MovieCount,
            ShowtimeCount = summary.ShowtimeCount,
            ActorCount = summary.ActorCount,
            UserCount = summary.UserCount,
            TicketCount = summary.TicketCount,
            Last7DaysSales = summary.Last7DaysSales,
            TopMovies = summary.TopMovies,
            RecentTickets = summary.RecentTickets,
            HallOccupancies = summary.HallOccupancies
        };

        return View(vm);
    }
}
