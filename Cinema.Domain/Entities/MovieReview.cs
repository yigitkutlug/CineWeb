using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

public class MovieReview
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = "";

    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    public bool IsApproved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
