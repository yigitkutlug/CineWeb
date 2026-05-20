using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

public class MovieReviewLike
{
    public int Id { get; set; }

    public int MovieReviewId { get; set; }
    public MovieReview MovieReview { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }
}
