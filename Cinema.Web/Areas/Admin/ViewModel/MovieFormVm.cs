using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Cinema.Web.Areas.Admin.ViewModels;

// Not: Entity yerine ViewModel kullanmamizin sebebi forma ozel alanlar ekleyebilmek.
// (SelectedActorIds, ActorOptions gibi alanlar Movie entity'sinde olmamali.)
public class MovieFormVm
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = "";

    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    [StringLength(80)]
    public string Genre { get; set; } = "";

    [StringLength(2000)]
    public string? Description { get; set; }

    public string? PosterImagePath { get; set; }

    public bool IsFeatured { get; set; }

    // Formda secilen oyuncularin id listesi (checkbox'lardan gelir).
    [Display(Name = "Oyuncular")]
    public List<int> SelectedActorIds { get; set; } = new();

    // Form render edilirken checkbox/option listesi icin kullanilir.
    public List<SelectListItem> ActorOptions { get; set; } = new();
}
