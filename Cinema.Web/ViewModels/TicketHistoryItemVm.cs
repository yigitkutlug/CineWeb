namespace Cinema.Web.ViewModels;

// Biletlerim ekraninda bir satiri temsil eder.
public class TicketHistoryItemVm
{
    public int TicketId { get; set; }
    public string MovieTitle { get; set; } = "";
    public string? SeatNo { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime? SessionAt { get; set; }
    public decimal Price { get; set; }
}
