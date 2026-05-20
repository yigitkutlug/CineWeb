using Cinema.Application.Movies;
using Cinema.Application.Reviews;

namespace Cinema.Web.ViewModels;

public class MovieDetailsPageVm
{
    public MovieDetailsDto Details { get; set; } = default!;
    public List<ReviewListItemDto> Reviews { get; set; } = new();
    public ReviewSummaryDto ReviewSummary { get; set; } = new();
    public ReviewListItemDto? MyReview { get; set; }
    public MovieReviewFormVm ReviewForm { get; set; } = new();
}
