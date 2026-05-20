using Cinema.Application.Tickets;
using Cinema.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Tickets;

public class TicketService : ITicketService
{
    private readonly AppDbContext _dbContext;

    public TicketService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TicketHistoryDto>> GetUserTicketsAsync(string userId)
    {
        return await _dbContext.Tickets
            .Where(t => t.UserId == userId)
            .Include(t => t.Movie)
            .OrderByDescending(t => t.PurchasedAt)
            .Select(t => new TicketHistoryDto
            {
                TicketId = t.Id,
                MovieTitle = t.Movie != null ? t.Movie.Title : "(Film silinmis)",
                SeatNo = t.SeatNo,
                PurchasedAtUtc = t.PurchasedAt,
                SessionAtUtc = t.SessionAt,
                Price = t.Price
            })
            .ToListAsync();
    }

    public async Task<List<AdminTicketItemDto>> GetUserTicketsForAdminAsync(string userId)
    {
        return await _dbContext.Tickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.PurchasedAt)
            .Select(t => new AdminTicketItemDto
            {
                TicketId = t.Id,
                MovieTitle = t.Movie != null ? t.Movie.Title : "",
                StartsAtUtc = t.Showtime != null ? t.Showtime.StartsAt : null,
                HallName = t.Showtime != null && t.Showtime.Hall != null ? t.Showtime.Hall.Name : "",
                SeatLabel = !string.IsNullOrWhiteSpace(t.SeatNo)
                    ? t.SeatNo
                    : (t.Seat != null ? t.Seat.RowLabel + t.Seat.SeatNumber : null),
                Price = t.Price,
                PurchasedAtUtc = t.PurchasedAt
            })
            .ToListAsync();
    }

    public async Task<TicketResultDto> CancelTicketAsync(int ticketId, string userId, DateTime nowUtc)
    {
        var ticket = await _dbContext.Tickets
            .Include(t => t.Showtime)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            return new TicketResultDto { Success = false, ErrorMessage = "Bilet bulunamadi." };

        if (ticket.UserId != userId)
            return new TicketResultDto { Success = false, ErrorMessage = "Yetkisiz islem." };

        var startsAtUtc = ticket.Showtime?.StartsAt;
        if (startsAtUtc.HasValue && startsAtUtc.Value <= nowUtc)
            return new TicketResultDto { Success = false, ErrorMessage = "Gecmis bilet iptal edilemez." };

        _dbContext.Tickets.Remove(ticket);
        await _dbContext.SaveChangesAsync();

        return new TicketResultDto { Success = true };
    }
}
