using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cinema.Domain.Entities;

// Oyuncu entity'si (veritabani tablosu modeli).
public class Actor
{
    // Primary key (Actors tablosundaki benzersiz id)
    public int Id { get; set; }

    // Formdan gelen "Ad" bilgisi
    [Required]
    [StringLength(60)]
    public string FirstName { get; set; } = "";

    // Formdan gelen "Soyad" bilgisi
    [Required]
    [StringLength(60)]
    public string LastName { get; set; } = "";

    [StringLength(120)]
    // Formda direkt alinmaz; controller'da FirstName + LastName birlestirilerek doldurulur.
    // Veritabaninda hazir tutulmasi listeleme/siralama/arama senaryolarini kolaylastirir.
    public string FullName { get; set; } = "";

    // Oyuncunun uyrugu (opsiyonel)
    [StringLength(80)]
    public string? Nationality { get; set; }

    // Dogum tarihi (opsiyonel). Controller'da UTC normalize edilir.
    public DateTime? BirthDate { get; set; }

    // Yuklenen fotograf dosyasinin web'den erisilebilir yolu (or: /uploads/actors/abc.jpg)
    [StringLength(300)]
    public string? PhotoPath { get; set; }

    // Oyuncu hakkinda kisa biyografi metni
    [StringLength(2000)]
    public string? Biography { get; set; }

    [NotMapped]
    // Veritabaninda tutulmaz; BirthDate'den her istekte hesaplanir.
    public int? Age
    {
        get
        {
            if (!BirthDate.HasValue) return null;
            var birth = BirthDate.Value.Date;
            var today = DateTime.UtcNow.Date;
            var age = today.Year - birth.Year;
            if (birth > today.AddYears(-age)) age--;
            return age < 0 ? null : age;
        }
    }

    // Many-to-many iliski navigation'i:
    // Bu oyuncunun oynadigi filmler MovieActor join tablosu uzerinden takip edilir.
    public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
}
