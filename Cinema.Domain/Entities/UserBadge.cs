using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

public class UserBadge
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public int BadgeId { get; set; }
    public Badge Badge { get; set; } = null!;

    public DateTime EarnedAtUtc { get; set; }
}
