namespace Cinema.Application.Dashboard;

public class AdminDashboardSummaryDto
{
    public int MovieCount { get; set; }
    public int ShowtimeCount { get; set; }
    public int ActorCount { get; set; }
    public int UserCount { get; set; }
    public int TicketCount { get; set; }
    public List<DashboardSalesPointDto> Last7DaysSales { get; set; } = new();
    public List<DashboardTopMovieDto> TopMovies { get; set; } = new();
    public List<DashboardRecentTicketDto> RecentTickets { get; set; } = new();
    public List<DashboardHallOccupancyDto> HallOccupancies { get; set; } = new();
}

public class DashboardSalesPointDto
{
    public DateTime DayUtc { get; set; }
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}

public class DashboardTopMovieDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}

public class DashboardRecentTicketDto
{
    public int TicketId { get; set; }
    public string UserDisplay { get; set; } = "";
    public string MovieTitle { get; set; } = "";
    public string HallName { get; set; } = "";
    public string SeatLabel { get; set; } = "";
    public decimal Price { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
}

public class DashboardHallOccupancyDto
{
    public int HallId { get; set; }
    public string HallName { get; set; } = "";
    public int TotalSeats { get; set; }
    public int TotalShowtimes { get; set; }
    public int SoldSeats { get; set; }
    public decimal OccupancyRate { get; set; }
}
