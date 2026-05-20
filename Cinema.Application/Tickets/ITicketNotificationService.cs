using Cinema.Domain.Entities;

namespace Cinema.Application.Tickets;

// Satin alma sonrasi bildirim/eposta gonderimi.
public interface ITicketNotificationService
{
    Task SendTicketPurchaseConfirmationAsync(string userId, Showtime showtime, List<string> seatNumbers, decimal totalPrice);
}
