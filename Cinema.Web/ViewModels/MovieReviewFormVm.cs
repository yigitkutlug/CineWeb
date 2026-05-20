using System.ComponentModel.DataAnnotations;

namespace Cinema.Web.ViewModels;

public class MovieReviewFormVm
{
    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [StringLength(2000)]
    public string? Comment { get; set; }
}
