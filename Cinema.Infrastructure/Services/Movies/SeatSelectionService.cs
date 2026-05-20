using Cinema.Application.Movies;
using Cinema.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Movies;

public class SeatSelectionService : ISeatSelectionService
{
    private readonly AppDbContext _dbContext;

    public SeatSelectionService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SeatSelectionResultDto> GetSeatSelectionAsync(int showtimeId, DateTime nowUtc, string? errorMessage)
    {
        var movieId = await _dbContext.Showtimes
            .Where(s => s.Id == showtimeId)
            .Select(s => s.MovieId)
            .FirstOrDefaultAsync();

        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == showtimeId && s.IsActive && s.StartsAt > nowUtc);

        if (showtime == null || showtime.Movie == null || showtime.Hall == null)
        {
            return new SeatSelectionResultDto
            {
                MovieId = movieId,
                Data = null
            };
        }

        var seats = await _dbContext.Seats
            .Where(s => s.HallId == showtime.HallId && s.IsActive)
            .OrderBy(s => s.RowLabel)
            .ThenByDescending(s => s.SeatNumber)
            .ToListAsync();

        var reservedSeatIds = await _dbContext.Tickets
            .Where(t => t.ShowtimeId == showtimeId && t.SeatId != null)
            .Select(t => t.SeatId!.Value)
            .ToListAsync();

        return new SeatSelectionResultDto
        {
            MovieId = movieId,
            Data = new SeatSelectionDto
            {
                Showtime = showtime,
                Movie = showtime.Movie,
                Hall = showtime.Hall,
                Seats = seats,
                ReservedSeatIds = reservedSeatIds.ToHashSet(),
                ErrorMessage = errorMessage,
                SelectedSeatIds = new List<int>()
            }
        };
    }
}
