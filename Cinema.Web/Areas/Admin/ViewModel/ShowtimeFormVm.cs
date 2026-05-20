using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cinema.Web.Areas.Admin.ViewModels;

public class ShowtimeFormVm
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";

    [Display(Name = "Salon")]
    public int HallId { get; set; }

    [Display(Name = "Baslangic")]
    public DateTime StartsAt { get; set; } = DateTime.Now.AddHours(2);

    [Display(Name = "Fiyat")]
    [Range(0, 100000)]
    public decimal Price { get; set; } = 220m;

    public bool IsActive { get; set; } = true;

    public bool ReturnToAllShowtimes { get; set; }

    public List<SelectListItem> MovieOptions { get; set; } = new();
    public List<SelectListItem> HallOptions { get; set; } = new();
}
