using System.ComponentModel.DataAnnotations;

namespace Cinema.Domain.Entities;

public class Badge
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = "";

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = "";

    [StringLength(300)]
    public string? Description { get; set; }

    [StringLength(200)]
    public string? RequirementText { get; set; }

    public List<UserBadge> UserBadges { get; set; } = new();
}
