namespace Cinema.Application.Movies;

// Seans koltuk secim ekranina veri hazirlar.
public interface ISeatSelectionService
{
    Task<SeatSelectionResultDto> GetSeatSelectionAsync(int showtimeId, DateTime nowUtc, string? errorMessage);
}
