using System.ComponentModel.DataAnnotations;

namespace Cinema.Web.ViewModels;

// Giris yapan kullanicinin Profilim ekraninda kullanilan form modeli.
public class ProfileEditVm
{
    public string UserId { get; set; } = "";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = "";

    [Phone]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Ad")]
    [StringLength(80)]
    public string? FirstName { get; set; }

    [Display(Name = "Soyad")]
    [StringLength(80)]
    public string? LastName { get; set; }

    [Display(Name = "Åehir")]
    [StringLength(100)]
    public string? City { get; set; }

    [Display(Name = "Adres")]
    [StringLength(500)]
    public string? Address { get; set; }

    public string? ProfilePhotoPath { get; set; }

    public List<BadgeItemVm> EarnedBadges { get; set; } = new();
    public List<BadgeItemVm> LockedBadges { get; set; } = new();
}

public class BadgeItemVm
{
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? RequirementText { get; set; }
    public DateTime? EarnedAtUtc { get; set; }
}
