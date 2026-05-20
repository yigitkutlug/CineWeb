using Cinema.Application.Badges;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Badges;

public class BadgeService : IBadgeService
{
    private readonly AppDbContext _dbContext;

    public BadgeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserBadgeListDto> GetUserBadgesAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new UserBadgeListDto();

        await EnsureBadgesAsync();

        var badges = await _dbContext.Badges
            .OrderBy(b => b.Id)
            .ToListAsync();

        var userBadges = await _dbContext.UserBadges
            .Where(ub => ub.UserId == userId)
            .ToListAsync();

        var ticketCount = await _dbContext.Tickets.CountAsync(t => t.UserId == userId);
        var reviewCount = await _dbContext.MovieReviews.CountAsync(r => r.UserId == userId);
        var ratingCount = await _dbContext.MovieReviews.CountAsync(r => r.UserId == userId && r.Rating >= 1 && r.Rating <= 5);
        var favoriteCount = 0;

        var badgeMap = badges.ToDictionary(b => b.Code, b => b.Id);
        var earnedBadgeIds = new HashSet<int>(userBadges.Select(ub => ub.BadgeId));

        bool Earns(string code)
        {
            return code switch
            {
                "FIRST_TICKET" => ticketCount >= 1,
                "LOYAL" => ticketCount >= 10,
                "REVIEWER" => reviewCount >= 5,
                "CRITIC" => reviewCount >= 20,
                "RATER" => ratingCount >= 1,
                "FAVORITER" => favoriteCount >= 5,
                _ => false
            };
        }

        var added = false;
        foreach (var badge in badges)
        {
            if (!earnedBadgeIds.Contains(badge.Id) && Earns(badge.Code))
            {
                _dbContext.UserBadges.Add(new UserBadge
                {
                    UserId = userId,
                    BadgeId = badge.Id,
                    EarnedAtUtc = DateTime.UtcNow
                });
                earnedBadgeIds.Add(badge.Id);
                added = true;
            }
        }

        if (added)
            await _dbContext.SaveChangesAsync();

        var userBadgeTimes = await _dbContext.UserBadges
            .Where(ub => ub.UserId == userId)
            .ToDictionaryAsync(ub => ub.BadgeId, ub => ub.EarnedAtUtc);

        var earned = new List<BadgeItemDto>();
        var locked = new List<BadgeItemDto>();

        foreach (var badge in badges)
        {
            var isEarned = userBadgeTimes.ContainsKey(badge.Id);
            var item = new BadgeItemDto
            {
                Code = badge.Code,
                Title = badge.Title,
                Description = badge.Description,
                RequirementText = badge.RequirementText,
                IsEarned = isEarned,
                EarnedAtUtc = isEarned ? userBadgeTimes[badge.Id] : null
            };

            if (isEarned)
                earned.Add(item);
            else
                locked.Add(item);
        }

        return new UserBadgeListDto
        {
            Earned = earned,
            Locked = locked
        };
    }

    private async Task EnsureBadgesAsync()
    {
        var definitions = new (string Code, string Title, string Description, string Requirement)[]
        {
            ("FIRST_TICKET", "Ilk Bilet", "Ilk biletini satin aldiginda kazanirsin.", "1 bilet"),
            ("REVIEWER", "Yorumcu", "5 yorum yaptiginda kazanirsin.", "5 yorum"),
            ("CRITIC", "Elestirmen", "20 yorum yaptiginda kazanirsin.", "20 yorum"),
            ("FAVORITER", "Favorici", "5 favori film eklediginde kazanirsin.", "5 favori"),
            ("RATER", "Puanlayici", "Ilk puan verdiginde kazanirsin.", "1 puan"),
            ("LOYAL", "Sadik Izleyici", "10 bilet satin aldiginda kazanirsin.", "10 bilet")
        };

        foreach (var def in definitions)
        {
            var exists = await _dbContext.Badges.AnyAsync(b => b.Code == def.Code);
            if (exists)
                continue;

            _dbContext.Badges.Add(new Badge
            {
                Code = def.Code,
                Title = def.Title,
                Description = def.Description,
                RequirementText = def.Requirement
            });
        }

        await _dbContext.SaveChangesAsync();
    }
}
