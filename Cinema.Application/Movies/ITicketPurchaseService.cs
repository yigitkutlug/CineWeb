namespace Cinema.Application.Movies;

// Bilet satin alma akisini yonetir.
public interface ITicketPurchaseService
{
    Task<PurchaseResultDto> BuySeatsAsync(int showtimeId, List<int> seatIds, string userId, DateTime nowUtc);
}
