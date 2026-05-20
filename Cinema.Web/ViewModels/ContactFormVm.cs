using System.ComponentModel.DataAnnotations;

namespace Cinema.Web.ViewModels;

public class ContactFormVm
{
    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(120)]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "GeÃ§erli bir e-posta gir.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Konu zorunludur.")]
    [StringLength(160)]
    public string Subject { get; set; } = "";

    [Required(ErrorMessage = "Mesaj zorunludur.")]
    [StringLength(2000)]
    public string Message { get; set; } = "";

    public bool IsSubmitted { get; set; }
}
