using Cinema.Application.Showtimes;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Showtimes;

public class ShowtimeAdminService : IShowtimeAdminService
{
    private readonly AppDbContext _dbContext;

    public ShowtimeAdminService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Movie?> GetMovieAsync(int movieId)
    {
        return await _dbContext.Movies.FirstOrDefaultAsync(m => m.Id == movieId);
    }

    public async Task<List<Movie>> GetMoviesAsync()
    {
        return await _dbContext.Movies
            .OrderBy(m => m.Title)
            .ToListAsync();
    }

    public async Task<List<Hall>> GetHallsAsync()
    {
        return await _dbContext.Halls
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<Showtime?> GetShowtimeAsync(int showtimeId)
    {
        return await _dbContext.Showtimes.FirstOrDefaultAsync(s => s.Id == showtimeId);
    }

    public async Task<Showtime?> GetShowtimeWithMovieAsync(int showtimeId)
    {
        return await _dbContext.Showtimes
            .Include(s => s.Movie)
            .FirstOrDefaultAsync(s => s.Id == showtimeId);
    }

    public async Task<List<Showtime>> GetShowtimesAsync(int? movieId)
    {
        var query = _dbContext.Showtimes.AsQueryable();
        if (movieId.HasValue)
            query = query.Where(s => s.MovieId == movieId.Value);

        return await query
            .Include(s => s.Hall)
            .Include(s => s.Movie)
            .OrderBy(s => s.StartsAt)
            .ToListAsync();
    }

    public async Task<bool> HasHallConflictAsync(int hallId, DateTime startsAtUtc, int durationMinutes, int? excludedShowtimeId = null)
    {
        var endsAtUtc = startsAtUtc.AddMinutes(durationMinutes);

        var existingShowtimes = await _dbContext.Showtimes
            .Where(s => s.HallId == hallId && (!excludedShowtimeId.HasValue || s.Id != excludedShowtimeId.Value))
            .Include(s => s.Movie)
            .ToListAsync();

        return existingShowtimes.Any(s =>
        {
            var existingDuration = s.Movie?.DurationMinutes ?? 0;
            var existingEndsAtUtc = s.StartsAt.AddMinutes(existingDuration);
            return startsAtUtc < existingEndsAtUtc && s.StartsAt < endsAtUtc;
        });
    }

    public async Task<int> CreateShowtimeAsync(Showtime showtime)
    {
        _dbContext.Showtimes.Add(showtime);
        await _dbContext.SaveChangesAsync();
        return showtime.Id;
    }

    public async Task UpdateShowtimeAsync(Showtime showtime)
    {
        _dbContext.Showtimes.Update(showtime);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteShowtimeAsync(int showtimeId)
    {
        var showtime = await _dbContext.Showtimes.FirstOrDefaultAsync(s => s.Id == showtimeId);
        if (showtime == null)
            return;

        _dbContext.Showtimes.Remove(showtime);
        await _dbContext.SaveChangesAsync();
    }
}
