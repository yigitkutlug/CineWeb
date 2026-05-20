using Cinema.Application.Dashboard;
using Cinema.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public AdminDashboardService(AppDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public async Task<AdminDashboardSummaryDto> GetSummaryAsync()
    {
        var nowUtc = DateTime.UtcNow;
        var startDayUtc = nowUtc.Date.AddDays(-6);
        var endDayUtc = nowUtc.Date.AddDays(1);

        var ticketCount = await _dbContext.Tickets.CountAsync();

        var salesRaw = await _dbContext.Tickets
            .Where(t => t.PurchasedAt >= startDayUtc && t.PurchasedAt < endDayUtc)
            .GroupBy(t => t.PurchasedAt.Date)
            .Select(g => new
            {
                Day = g.Key,
                Count = g.Count(),
                Revenue = g.Sum(x => x.Price)
            })
            .ToListAsync();

        var sales = new List<DashboardSalesPointDto>();
        for (var i = 0; i < 7; i++)
        {
            var day = startDayUtc.AddDays(i);
            var item = salesRaw.FirstOrDefault(x => x.Day == day);
            sales.Add(new DashboardSalesPointDto
            {
                DayUtc = day,
                TicketCount = item?.Count ?? 0,
                Revenue = item?.Revenue ?? 0m
            });
        }

        var topMoviesRaw = await _dbContext.Tickets
            .Where(t => t.MovieId != null)
            .GroupBy(t => t.MovieId!.Value)
            .Select(g => new
            {
                MovieId = g.Key,
                Count = g.Count(),
                Revenue = g.Sum(x => x.Price)
            })
            .OrderByDescending(x => x.Count)
            .ThenByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();

        var movieTitles = await _dbContext.Movies
            .Where(m => topMoviesRaw.Select(x => x.MovieId).Contains(m.Id))
            .Select(m => new { m.Id, m.Title })
            .ToListAsync();

        var topMovies = topMoviesRaw.Select(x => new DashboardTopMovieDto
        {
            MovieId = x.MovieId,
            MovieTitle = movieTitles.FirstOrDefault(m => m.Id == x.MovieId)?.Title ?? "Film",
            TicketCount = x.Count,
            Revenue = x.Revenue
        }).ToList();

        var recentTicketsRaw = await _dbContext.Tickets
            .Include(t => t.Movie)
            .Include(t => t.Showtime)
                .ThenInclude(s => s!.Hall)
            .OrderByDescending(t => t.PurchasedAt)
            .Take(8)
            .Select(t => new
            {
                t.Id,
                t.UserId,
                MovieTitle = t.Movie != null ? t.Movie.Title : "",
                HallName = t.Showtime != null && t.Showtime.Hall != null ? t.Showtime.Hall.Name : "",
                SeatLabel = t.SeatNo,
                t.Price,
                t.PurchasedAt
            })
            .ToListAsync();

        var userIds = recentTicketsRaw.Select(x => x.UserId).Distinct().ToList();
        var users = await _userManager.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToListAsync();

        var recentTickets = recentTicketsRaw.Select(t => new DashboardRecentTicketDto
        {
            TicketId = t.Id,
            UserDisplay = users.FirstOrDefault(u => u.Id == t.UserId)?.Email ?? "Kullanici",
            MovieTitle = string.IsNullOrWhiteSpace(t.MovieTitle) ? "(Film silinmis)" : t.MovieTitle,
            HallName = string.IsNullOrWhiteSpace(t.HallName) ? "-" : t.HallName,
            SeatLabel = string.IsNullOrWhiteSpace(t.SeatLabel) ? "-" : t.SeatLabel,
            Price = t.Price,
            PurchasedAtUtc = t.PurchasedAt
        }).ToList();

        var hallSeats = await _dbContext.Seats
            .GroupBy(s => s.HallId)
            .Select(g => new { HallId = g.Key, SeatCount = g.Count() })
            .ToListAsync();

        var upcomingEndUtc = nowUtc.AddDays(7);
        var upcomingShowtimes = await _dbContext.Showtimes
            .Where(s => s.IsActive && s.StartsAt >= nowUtc && s.StartsAt < upcomingEndUtc)
            .Select(s => new { s.Id, s.HallId })
            .ToListAsync();

        var showtimeIds = upcomingShowtimes.Select(x => x.Id).ToList();
        var showtimeHallMap = upcomingShowtimes.ToDictionary(x => x.Id, x => x.HallId);

        var soldShowtimeIds = await _dbContext.Tickets
            .Where(t => t.ShowtimeId.HasValue && showtimeIds.Contains(t.ShowtimeId.Value))
            .Select(t => t.ShowtimeId!.Value)
            .ToListAsync();

        var soldPerHall = soldShowtimeIds
            .GroupBy(id => showtimeHallMap[id])
            .ToDictionary(g => g.Key, g => g.Count());

        var showtimesPerHall = upcomingShowtimes
            .GroupBy(x => x.HallId)
            .ToDictionary(g => g.Key, g => g.Count());

        var halls = await _dbContext.Halls
            .OrderBy(h => h.Name)
            .Select(h => new { h.Id, h.Name })
            .ToListAsync();

        var hallOccupancies = halls.Select(h =>
        {
            var seatCount = hallSeats.FirstOrDefault(s => s.HallId == h.Id)?.SeatCount ?? 0;
            var showtimeCount = showtimesPerHall.ContainsKey(h.Id) ? showtimesPerHall[h.Id] : 0;
            var sold = soldPerHall.ContainsKey(h.Id) ? soldPerHall[h.Id] : 0;

            // Toplam doluluk: satilan koltuk / (koltuk * seans)
            var capacity = seatCount * showtimeCount;
            var rate = capacity == 0 ? 0m : Math.Round((decimal)sold / capacity * 100m, 1);

            return new DashboardHallOccupancyDto
            {
                HallId = h.Id,
                HallName = h.Name,
                TotalSeats = seatCount,
                TotalShowtimes = showtimeCount,
                SoldSeats = sold,
                OccupancyRate = rate
            };
        }).ToList();

        return new AdminDashboardSummaryDto
        {
            MovieCount = await _dbContext.Movies.CountAsync(),
            ShowtimeCount = await _dbContext.Showtimes.CountAsync(),
            ActorCount = await _dbContext.Actors.CountAsync(),
            UserCount = await _userManager.Users.CountAsync(),
            TicketCount = ticketCount,
            Last7DaysSales = sales,
            TopMovies = topMovies,
            RecentTickets = recentTickets,
            HallOccupancies = hallOccupancies
        };
    }
}
