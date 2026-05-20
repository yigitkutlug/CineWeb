namespace Cinema.Application.Showtimes;

// Public/API tarafindan kullanilan seans sorgulari.
public interface IShowtimeQueryService
{
    Task<List<ShowtimeListDto>> GetShowtimeListForApiAsync(DateTime nowUtc);
    Task<ShowtimeDetailDto?> GetShowtimeDetailForApiAsync(int showtimeId, DateTime nowUtc);
}
