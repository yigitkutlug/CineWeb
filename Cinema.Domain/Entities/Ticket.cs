using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

// Kullanici satin alma/bilet gecmisi kaydi.
// Koltuk kilitleme icin ShowtimeId + SeatId kombinasyonu DB'de unique tutulur.
public class Ticket
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public int? MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int? ShowtimeId { get; set; }
    public Showtime? Showtime { get; set; }

    public int? SeatId { get; set; }
    public Seat? Seat { get; set; }

    [StringLength(20)]
    public string? SeatNo { get; set; }

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SessionAt { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }
}
