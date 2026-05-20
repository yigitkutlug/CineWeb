namespace Cinema.Application.Tickets;

public interface ITicketService
{
    Task<List<TicketHistoryDto>> GetUserTicketsAsync(string userId);
    Task<List<AdminTicketItemDto>> GetUserTicketsForAdminAsync(string userId);
    Task<TicketResultDto> CancelTicketAsync(int ticketId, string userId, DateTime nowUtc);
}
