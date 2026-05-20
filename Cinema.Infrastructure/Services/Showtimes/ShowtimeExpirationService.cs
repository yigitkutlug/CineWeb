using Cinema.Application.Showtimes;
using Cinema.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Showtimes;

public class ShowtimeExpirationService : IShowtimeExpirationService
{
    private readonly AppDbContext _dbContext;

    public ShowtimeExpirationService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> ExpireAsync(CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var candidates = await _dbContext.Showtimes
            .Where(s => s.IsActive && s.StartsAt <= nowUtc)
            .Include(s => s.Movie)
            .ToListAsync(ct);

        if (candidates.Count == 0)
            return 0;

        var expiredCount = 0;
        foreach (var showtime in candidates)
        {
            var durationMinutes = showtime.Movie?.DurationMinutes ?? 0;
            var endsAtUtc = showtime.StartsAt.AddMinutes(durationMinutes);
            if (endsAtUtc <= nowUtc)
            {
                showtime.IsActive = false;
                expiredCount++;
            }
        }

        if (expiredCount > 0)
            await _dbContext.SaveChangesAsync(ct);

        return expiredCount;
    }
}
