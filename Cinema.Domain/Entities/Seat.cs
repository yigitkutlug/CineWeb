using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

// Bir salon icindeki tekil koltuk.
// Ayni salon icinde RowLabel + SeatNumber kombinasyonu benzersizdir.
public class Seat
{
    public int Id { get; set; }

    public int HallId { get; set; }
    public Hall? Hall { get; set; }

    [Required]
    [StringLength(2)]
    public string RowLabel { get; set; } = "";

    [Range(1, 100)]
    public int SeatNumber { get; set; }

    // Fiziksel olarak kullanilmayacak koltuklar icin.
    public bool IsActive { get; set; } = true;

    // Aisle/gorsel bosluk icin satir icinde ekstra margin kullanmak istersek.
    public bool IsAisleAfter { get; set; }

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
