namespace Cinema.Application.Showtimes;

public interface IShowtimeExpirationService
{
    Task<int> ExpireAsync(CancellationToken ct);
}
