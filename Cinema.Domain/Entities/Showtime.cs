using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

// Filme ait seans (tarih/saat + salon + fiyat).
// Bilet satin alma koltuk secimi her zaman bir Showtime uzerinden yapilir.
public class Showtime
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public Movie? Movie { get; set; }

    public int HallId { get; set; }
    public Hall? Hall { get; set; }

    public DateTime StartsAt { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; } = 220m;

    public bool IsActive { get; set; } = true;

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
