using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

public class MovieReviewReply
{
    public int Id { get; set; }

    public int MovieReviewId { get; set; }
    public MovieReview MovieReview { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = "";

    [Required]
    [StringLength(1000)]
    public string Comment { get; set; } = "";

    public bool IsApproved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
}
