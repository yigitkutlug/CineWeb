namespace Cinema.Web.Areas.Admin.ViewModels;

// Users/Edit ekraninin form modeli.
public class UserEditVm
{
    // IdentityUser primary key (Identity'de string).
    public string Id { get; set; } = "";
    // Email ve ayni zamanda username olarak kullaniliyor.
    public string Email { get; set; } = "";
    // Kullanici telefon bilgisi (opsiyonel)
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Address { get; set; }
    public string? ProfilePhotoPath { get; set; }

    // Admin rol checkbox durumu.
    public bool IsAdmin { get; set; }
    // SuperAdmin rol checkbox durumu.
    public bool IsSuperAdmin { get; set; }
}
