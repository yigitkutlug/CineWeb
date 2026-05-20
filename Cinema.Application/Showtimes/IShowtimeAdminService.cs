using Cinema.Domain.Entities;

namespace Cinema.Application.Showtimes;

// Admin CRUD ve salon/film sorgulari.
public interface IShowtimeAdminService
{
    Task<Movie?> GetMovieAsync(int movieId);
    Task<List<Movie>> GetMoviesAsync();
    Task<List<Hall>> GetHallsAsync();
    Task<Showtime?> GetShowtimeAsync(int showtimeId);
    Task<Showtime?> GetShowtimeWithMovieAsync(int showtimeId);
    Task<List<Showtime>> GetShowtimesAsync(int? movieId);
    Task<bool> HasHallConflictAsync(int hallId, DateTime startsAtUtc, int durationMinutes, int? excludedShowtimeId = null);
    Task<int> CreateShowtimeAsync(Showtime showtime);
    Task UpdateShowtimeAsync(Showtime showtime);
    Task DeleteShowtimeAsync(int showtimeId);
}
