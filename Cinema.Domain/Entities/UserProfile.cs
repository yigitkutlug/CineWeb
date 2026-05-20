using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

// Identity kullanicisinin profil detaylari (1 kullanici = 1 profil).
public class UserProfile
{
    [Key]
    public string UserId { get; set; } = "";

    [StringLength(80)]
    public string? FirstName { get; set; }

    [StringLength(80)]
    public string? LastName { get; set; }

    [StringLength(50)]
    public string? UserName { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    // wwwroot altindaki relatif dosya yolu veya URL tutulabilir.
    [StringLength(300)]
    public string? ProfilePhotoPath { get; set; }
}
