namespace Cinema.Application.Badges;

public interface IBadgeService
{
    Task<UserBadgeListDto> GetUserBadgesAsync(string userId);
}
