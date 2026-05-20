namespace Cinema.Application.Badges;

public class BadgeItemDto
{
    public string Code { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public string? RequirementText { get; set; }
    public bool IsEarned { get; set; }
    public DateTime? EarnedAtUtc { get; set; }
}

public class UserBadgeListDto
{
    public List<BadgeItemDto> Earned { get; set; } = new();
    public List<BadgeItemDto> Locked { get; set; } = new();
}
