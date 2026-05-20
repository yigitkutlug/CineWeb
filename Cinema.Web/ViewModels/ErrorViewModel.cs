namespace Cinema.Web.ViewModels;

// Hata ekranina giden minimal veri modeli (request id gibi debug bilgileri).
public class ErrorViewModel
{
    // Sunucudan gelen hata istegini takip etmek icin kullanilir.
    public string? RequestId { get; set; }

    // RequestId bos degilse ekranda gosterilsin.
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
