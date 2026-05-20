using Cinema.Application.Showtimes;
using Cinema.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Showtimes;

public class ShowtimeQueryService : IShowtimeQueryService
{
    private readonly AppDbContext _dbContext;

    public ShowtimeQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ShowtimeListDto>> GetShowtimeListForApiAsync(DateTime nowUtc)
    {
        return await _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .OrderBy(s => s.StartsAt)
            .Select(s => new ShowtimeListDto
            {
                Id = s.Id,
                MovieTitle = s.Movie != null ? s.Movie.Title : "",
                HallName = s.Hall != null ? s.Hall.Name : "",
                StartsAtUtc = s.StartsAt,
                Price = s.Price,
                IsActive = s.IsActive
            })
            .ToListAsync();
    }

    public async Task<ShowtimeDetailDto?> GetShowtimeDetailForApiAsync(int showtimeId, DateTime nowUtc)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == showtimeId);

        if (showtime == null)
            return null;

        return new ShowtimeDetailDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            MovieTitle = showtime.Movie?.Title ?? "",
            HallName = showtime.Hall?.Name ?? "",
            StartsAtUtc = showtime.StartsAt,
            Price = showtime.Price,
            IsActive = showtime.IsActive
        };
    }
}
