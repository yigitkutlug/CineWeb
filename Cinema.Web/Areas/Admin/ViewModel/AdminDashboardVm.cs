namespace Cinema.Web.Areas.Admin.ViewModels;

// Admin ve SuperAdmin icin ortak dashboard ozeti.
public class AdminDashboardVm
{
    public int MovieCount { get; set; }
    public int ShowtimeCount { get; set; }
    public int ActorCount { get; set; }
    public int UserCount { get; set; }
    public int TicketCount { get; set; }
    public List<Cinema.Application.Dashboard.DashboardSalesPointDto> Last7DaysSales { get; set; } = new();
    public List<Cinema.Application.Dashboard.DashboardTopMovieDto> TopMovies { get; set; } = new();
    public List<Cinema.Application.Dashboard.DashboardRecentTicketDto> RecentTickets { get; set; } = new();
    public List<Cinema.Application.Dashboard.DashboardHallOccupancyDto> HallOccupancies { get; set; } = new();
}
