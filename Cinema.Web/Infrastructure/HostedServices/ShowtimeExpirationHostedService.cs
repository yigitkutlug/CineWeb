using Cinema.Application.Showtimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cinema.Web.Infrastructure.HostedServices;

public sealed class ShowtimeExpirationHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ShowtimeExpirationHostedService> _logger;

    public ShowtimeExpirationHostedService(IServiceScopeFactory scopeFactory, ILogger<ShowtimeExpirationHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ExpireShowtimesAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ExpireShowtimesAsync(stoppingToken);
        }
    }

    private async Task ExpireShowtimesAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var expirationService = scope.ServiceProvider.GetRequiredService<IShowtimeExpirationService>();
            await expirationService.ExpireAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to expire showtimes.");
        }
    }
}
