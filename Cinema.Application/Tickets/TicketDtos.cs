namespace Cinema.Application.Tickets;

public class TicketPurchaseDto
{
    public int ShowtimeId { get; set; }
    public List<int> SeatIds { get; set; } = new();
}

public class TicketResultDto
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TicketHistoryDto
{
    public int TicketId { get; set; }
    public string MovieTitle { get; set; } = "";
    public string? SeatNo { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
    public DateTime? SessionAtUtc { get; set; }
    public decimal Price { get; set; }
}

public class AdminTicketItemDto
{
    public int TicketId { get; set; }
    public string MovieTitle { get; set; } = "";
    public DateTime? StartsAtUtc { get; set; }
    public string? HallName { get; set; }
    public string? SeatLabel { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
}
