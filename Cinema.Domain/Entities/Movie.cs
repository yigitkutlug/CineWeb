using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

// Film entity'si (veritabani tablosu modeli).
public class Movie
{
    // Primary key (Movies tablosundaki benzersiz id)
    public int Id { get; set; }
    // Film adi (zorunlu)
    [Required]
    [StringLength(150)]
    public string Title { get; set; } = null!;
    // Sure (dakika). Validation ile mantikli aralikta tutulur.
    [Range(1, 600)]
    public int DurationMinutes { get; set; }
    // Tur (aksiyon, komedi vb.)
    [StringLength(80)]
    public string Genre { get; set; } = "";
    // Film aciklamasi (public detay sayfasinda gosterilir)
    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(300)]
    public string? PosterImagePath { get; set; }

    // Vitrinde on plana cikarmak icin basit isaret.
    public bool IsFeatured { get; set; }

    // Many-to-many navigation:
    // Filmde oynayan oyunculari MovieActor join kayitlari uzerinden tutar.
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();

    // One-to-many navigation:
    // Bir filmin birden fazla seansi olabilir.
    public ICollection<Showtime> Showtimes { get; set; } = new List<Showtime>();

    // One-to-many navigation:
    // Film yorumlari (kullanici yorumlari)
    public ICollection<MovieReview> Reviews { get; set; } = new List<MovieReview>();

}
