using Cinema.Application.Movies;
using Cinema.Application.Tickets;
using Cinema.Domain.Entities;
using Cinema.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Movies;

public class TicketPurchaseService : ITicketPurchaseService
{
    private readonly AppDbContext _dbContext;
    private readonly ITicketNotificationService _notificationService;

    public TicketPurchaseService(AppDbContext dbContext, ITicketNotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
    }

    public async Task<PurchaseResultDto> BuySeatsAsync(int showtimeId, List<int> seatIds, string userId, DateTime nowUtc)
    {
        var showtime = await _dbContext.Showtimes
            .Include(s => s.Movie)
            .Include(s => s.Hall)
            .FirstOrDefaultAsync(s => s.Id == showtimeId && s.IsActive && s.StartsAt > nowUtc);

        if (showtime == null || showtime.Movie == null || showtime.Hall == null)
            return new PurchaseResultDto { Success = false };

        var normalizedSeatIds = seatIds
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (!normalizedSeatIds.Any())
        {
            return new PurchaseResultDto
            {
                Success = false,
                ErrorMessage = "En az bir koltuk secmelisin."
            };
        }

        var seats = await _dbContext.Seats
            .Where(s => normalizedSeatIds.Contains(s.Id) && s.HallId == showtime.HallId && s.IsActive)
            .OrderBy(s => s.RowLabel)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync();

        if (seats.Count != normalizedSeatIds.Count)
        {
            return new PurchaseResultDto
            {
                Success = false,
                ErrorMessage = "Secilen koltuklardan biri bulunamadi."
            };
        }

        var takenSeatIds = await _dbContext.Tickets
            .Where(t => t.ShowtimeId == showtimeId && t.SeatId != null && normalizedSeatIds.Contains(t.SeatId.Value))
            .Select(t => t.SeatId!.Value)
            .ToListAsync();

        if (takenSeatIds.Any())
        {
            return new PurchaseResultDto
            {
                Success = false,
                ErrorMessage = "Secilen koltuklardan biri artik dolu. Lutfen tekrar secim yap."
            };
        }

        var purchasedAt = DateTime.UtcNow;
        var seatNumbers = seats
            .Select(seat => $"{seat.RowLabel}{seat.SeatNumber}")
            .ToList();

        var tickets = seats.Select(seat => new Ticket
        {
            UserId = userId,
            MovieId = showtime.MovieId,
            ShowtimeId = showtime.Id,
            SeatId = seat.Id,
            SeatNo = $"{seat.RowLabel}{seat.SeatNumber}",
            PurchasedAt = purchasedAt,
            SessionAt = showtime.StartsAt,
            Price = showtime.Price
        }).ToList();

        _dbContext.Tickets.AddRange(tickets);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return new PurchaseResultDto
            {
                Success = false,
                ErrorMessage = "Secilen koltuklardan biri az once baska biri tarafindan alindi. Lutfen tekrar secim yap."
            };
        }

        await _notificationService.SendTicketPurchaseConfirmationAsync(userId, showtime, seatNumbers, tickets.Sum(t => t.Price));

        return new PurchaseResultDto
        {
            Success = true,
            SuccessPayload = new TicketPurchaseSuccessDto
            {
                MovieTitle = showtime.Movie.Title,
                HallName = showtime.Hall.Name,
                SessionAt = showtime.StartsAt,
                SeatNumbers = seatNumbers,
                TotalPrice = tickets.Sum(t => t.Price)
            }
        };
    }
}
