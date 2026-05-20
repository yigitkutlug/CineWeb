using Cinema.Application.Reviews;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Reviews;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _dbContext;

    public ReviewService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ReviewListItemDto>> GetApprovedForMovieAsync(int movieId, string? currentUserId)
    {
        var items = await _dbContext.MovieReviews
            .Where(x => x.MovieId == movieId && x.IsApproved)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ReviewListItemDto
            {
                Id = x.Id,
                MovieId = x.MovieId,
                UserId = x.UserId,
                DisplayName = "",
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAtUtc = x.CreatedAtUtc,
                LikeCount = 0,
                IsLikedByCurrentUser = false,
                Replies = new List<ReviewReplyDto>()
            })
            .ToListAsync();

        if (items.Count == 0)
            return items;

        var reviewIds = items.Select(x => x.Id).ToList();

        var userIds = items.Select(x => x.UserId).Distinct().ToList();
        var displayMap = await GetDisplayNameMapAsync(userIds);

        foreach (var item in items)
        {
            if (displayMap.TryGetValue(item.UserId, out var name))
                item.DisplayName = name;
        }

        var likes = await _dbContext.MovieReviewLikes
            .Where(l => reviewIds.Contains(l.MovieReviewId))
            .GroupBy(l => l.MovieReviewId)
            .Select(g => new { ReviewId = g.Key, Count = g.Count() })
            .ToListAsync();

        var likerUserIds = await _dbContext.MovieReviewLikes
            .Where(l => reviewIds.Contains(l.MovieReviewId))
            .Select(l => l.UserId)
            .Distinct()
            .ToListAsync();

        var likerNamesMap = await GetDisplayNameMapAsync(likerUserIds);
        var likerNamesRaw = await _dbContext.MovieReviewLikes
            .Where(l => reviewIds.Contains(l.MovieReviewId))
            .Select(l => new { l.MovieReviewId, l.UserId })
            .ToListAsync();

        var likedByMe = new HashSet<int>();
        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            likedByMe = (await _dbContext.MovieReviewLikes
                .Where(l => l.UserId == currentUserId && reviewIds.Contains(l.MovieReviewId))
                .Select(l => l.MovieReviewId)
                .ToListAsync()).ToHashSet();
        }

        foreach (var item in items)
        {
            item.LikeCount = likes.FirstOrDefault(l => l.ReviewId == item.Id)?.Count ?? 0;
            item.IsLikedByCurrentUser = likedByMe.Contains(item.Id);
            item.LikerNames = likerNamesRaw
                .Where(l => l.MovieReviewId == item.Id)
                .Select(l => likerNamesMap.TryGetValue(l.UserId, out var name) ? name : "Kullanici")
                .Distinct()
                .ToList();
        }

        var repliesRaw = await _dbContext.MovieReviewReplies
            .Where(r => reviewIds.Contains(r.MovieReviewId) && r.IsApproved)
            .OrderBy(r => r.CreatedAtUtc)
            .Select(r => new
            {
                r.Id,
                r.MovieReviewId,
                r.UserId,
                r.Comment,
                r.CreatedAtUtc
            })
            .ToListAsync();

        if (repliesRaw.Count > 0)
        {
            var replyUserIds = repliesRaw.Select(r => r.UserId).Distinct().ToList();
            var replyDisplay = await GetDisplayNameMapAsync(replyUserIds);

            var grouped = repliesRaw
                .GroupBy(r => r.MovieReviewId)
                .ToDictionary(g => g.Key, g => g.Select(r => new ReviewReplyDto
                {
                    Id = r.Id,
                    ReviewId = r.MovieReviewId,
                    UserId = r.UserId,
                    DisplayName = replyDisplay.TryGetValue(r.UserId, out var name) ? name : "Kullanici",
                    Comment = r.Comment,
                    CreatedAtUtc = r.CreatedAtUtc
                }).ToList());

            foreach (var item in items)
            {
                if (grouped.TryGetValue(item.Id, out var list))
                    item.Replies = list;
            }
        }

        return items;
    }

    public async Task<ReviewSummaryDto> GetApprovedSummaryAsync(int movieId)
    {
        var query = _dbContext.MovieReviews.Where(x => x.MovieId == movieId && x.IsApproved);
        var count = await query.CountAsync();
        var average = count == 0
            ? 0
            : await query.AverageAsync(x => (double)x.Rating);

        return new ReviewSummaryDto
        {
            ApprovedReviewCount = count,
            AverageRating = average
        };
    }

    public async Task<ReviewListItemDto?> GetUserReviewAsync(int movieId, string userId)
    {
        var item = await _dbContext.MovieReviews
            .Where(x => x.MovieId == movieId && x.UserId == userId)
            .Select(x => new ReviewListItemDto
            {
                Id = x.Id,
                MovieId = x.MovieId,
                UserId = x.UserId,
                DisplayName = "",
                Rating = x.Rating,
                Comment = x.Comment,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return null;

        var profile = await _dbContext.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                p.UserName,
                FullName = ((p.FirstName ?? "") + " " + (p.LastName ?? "")).Trim()
            })
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(profile?.FullName))
        {
            item.DisplayName = profile.FullName;
            return item;
        }

        if (!string.IsNullOrWhiteSpace(profile?.UserName))
        {
            item.DisplayName = profile.UserName;
            return item;
        }

        var email = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => u.Email)
            .FirstOrDefaultAsync();

        item.DisplayName = string.IsNullOrWhiteSpace(email) ? "Kullanici" : email;
        return item;
    }

    public async Task<ReviewResultDto> CreateOrUpdateAsync(int movieId, string userId, ReviewCreateDto dto)
    {
        return await UpsertInternalAsync(movieId, userId, dto.Rating, dto.Comment);
    }

    public async Task<ReviewResultDto> UpdateAsync(int movieId, string userId, ReviewUpdateDto dto)
    {
        return await UpsertInternalAsync(movieId, userId, dto.Rating, dto.Comment);
    }

    public async Task<ReviewResultDto> DeleteAsync(int movieId, string userId)
    {
        var review = await _dbContext.MovieReviews
            .FirstOrDefaultAsync(x => x.MovieId == movieId && x.UserId == userId);
        if (review == null)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yorum bulunamadi." } };

        _dbContext.MovieReviews.Remove(review);
        await _dbContext.SaveChangesAsync();
        return new ReviewResultDto { Success = true };
    }

    public async Task<List<ReviewAdminItemDto>> GetPendingAsync()
    {
        var items = await _dbContext.MovieReviews
            .Include(x => x.Movie)
            .Where(x => !x.IsApproved)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ReviewAdminItemDto
            {
                Id = x.Id,
                MovieId = x.MovieId,
                MovieTitle = x.Movie.Title,
                UserId = x.UserId,
                DisplayName = "",
                Rating = x.Rating,
                Comment = x.Comment,
                IsApproved = x.IsApproved,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync();

        await FillDisplayNamesAsync(items);
        return items;
    }

    public async Task<List<ReviewAdminItemDto>> GetAllForMovieAsync(int movieId)
    {
        var items = await _dbContext.MovieReviews
            .Include(x => x.Movie)
            .Where(x => x.MovieId == movieId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ReviewAdminItemDto
            {
                Id = x.Id,
                MovieId = x.MovieId,
                MovieTitle = x.Movie.Title,
                UserId = x.UserId,
                DisplayName = "",
                Rating = x.Rating,
                Comment = x.Comment,
                IsApproved = x.IsApproved,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync();

        await FillDisplayNamesAsync(items);
        return items;
    }

    public async Task<List<ReviewAdminItemDto>> GetForUserAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return new List<ReviewAdminItemDto>();

        var items = await _dbContext.MovieReviews
            .Include(x => x.Movie)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ReviewAdminItemDto
            {
                Id = x.Id,
                MovieId = x.MovieId,
                MovieTitle = x.Movie.Title,
                UserId = x.UserId,
                DisplayName = "",
                Rating = x.Rating,
                Comment = x.Comment,
                IsApproved = x.IsApproved,
                CreatedAtUtc = x.CreatedAtUtc,
                UpdatedAtUtc = x.UpdatedAtUtc
            })
            .ToListAsync();

        return items;
    }

    public async Task<List<ReviewModerationMovieDto>> GetMoviesForModerationAsync()
    {
        var items = await _dbContext.MovieReviews
            .Include(x => x.Movie)
            .GroupBy(x => new { x.MovieId, x.Movie.Title })
            .Select(g => new ReviewModerationMovieDto
            {
                MovieId = g.Key.MovieId,
                MovieTitle = g.Key.Title,
                TotalCount = g.Count(),
                ApprovedCount = g.Count(x => x.IsApproved),
                PendingCount = g.Count(x => !x.IsApproved),
                LastReviewAtUtc = g.Max(x => (DateTime?)x.CreatedAtUtc),
                ReplyCount = 0,
                PendingReplyCount = 0,
                LastReplyAtUtc = null
            })
            .OrderByDescending(x => x.LastReviewAtUtc)
            .ToListAsync();

        if (items.Count == 0)
            return items;

        var movieIds = items.Select(x => x.MovieId).ToList();

        var repliesAgg = await _dbContext.MovieReviewReplies
            .Include(r => r.MovieReview)
            .Where(r => movieIds.Contains(r.MovieReview.MovieId))
            .GroupBy(r => r.MovieReview.MovieId)
            .Select(g => new
            {
                MovieId = g.Key,
                ReplyCount = g.Count(),
                PendingReplyCount = g.Count(x => !x.IsApproved),
                LastReplyAtUtc = g.Max(x => (DateTime?)x.CreatedAtUtc)
            })
            .ToListAsync();

        foreach (var item in items)
        {
            var rep = repliesAgg.FirstOrDefault(r => r.MovieId == item.MovieId);
            if (rep == null) continue;
            item.ReplyCount = rep.ReplyCount;
            item.PendingReplyCount = rep.PendingReplyCount;
            item.LastReplyAtUtc = rep.LastReplyAtUtc;
        }

        return items;
    }

    public async Task<ReviewResultDto> SetApprovalAsync(int reviewId, bool isApproved)
    {
        var review = await _dbContext.MovieReviews.FirstOrDefaultAsync(x => x.Id == reviewId);
        if (review == null)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yorum bulunamadi." } };

        review.IsApproved = isApproved;
        review.UpdatedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return new ReviewResultDto { Success = true };
    }

    public async Task<ReviewResultDto> DeleteByIdAsync(int reviewId)
    {
        var review = await _dbContext.MovieReviews.FirstOrDefaultAsync(x => x.Id == reviewId);
        if (review == null)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yorum bulunamadi." } };

        _dbContext.MovieReviews.Remove(review);
        await _dbContext.SaveChangesAsync();
        return new ReviewResultDto { Success = true };
    }

    private async Task<ReviewResultDto> UpsertInternalAsync(int movieId, string userId, int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Puan 1 ile 5 arasinda olmali." } };

        var movieExists = await _dbContext.Movies.AnyAsync(x => x.Id == movieId);
        if (!movieExists)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Film bulunamadi." } };

        var review = await _dbContext.MovieReviews
            .FirstOrDefaultAsync(x => x.MovieId == movieId && x.UserId == userId);

        if (review == null)
        {
            review = new MovieReview
            {
                MovieId = movieId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                IsApproved = false,
                CreatedAtUtc = DateTime.UtcNow
            };
            _dbContext.MovieReviews.Add(review);
        }
        else
        {
            review.Rating = rating;
            review.Comment = comment;
            review.IsApproved = false;
            review.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return new ReviewResultDto { Success = true };
    }

    public async Task<ReviewResultDto> AddReplyAsync(int reviewId, string userId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yanit bos olamaz." } };

        var review = await _dbContext.MovieReviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.IsApproved);
        if (review == null)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yorum bulunamadi." } };

        _dbContext.MovieReviewReplies.Add(new MovieReviewReply
        {
            MovieReviewId = reviewId,
            UserId = userId,
            Comment = comment.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            IsApproved = false
        });

        await _dbContext.SaveChangesAsync();
        return new ReviewResultDto { Success = true };
    }

    public async Task<bool> ToggleLikeAsync(int reviewId, string userId)
    {
        var review = await _dbContext.MovieReviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.IsApproved);
        if (review == null)
            return false;

        var existing = await _dbContext.MovieReviewLikes
            .FirstOrDefaultAsync(l => l.MovieReviewId == reviewId && l.UserId == userId);

        if (existing != null)
        {
            _dbContext.MovieReviewLikes.Remove(existing);
        }
        else
        {
            _dbContext.MovieReviewLikes.Add(new MovieReviewLike
            {
                MovieReviewId = reviewId,
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ReviewLikeInfoDto?> GetLikeInfoAsync(int reviewId, string? currentUserId)
    {
        var exists = await _dbContext.MovieReviews.AnyAsync(r => r.Id == reviewId && r.IsApproved);
        if (!exists)
            return null;

        var likeCount = await _dbContext.MovieReviewLikes.CountAsync(l => l.MovieReviewId == reviewId);

        var liked = false;
        if (!string.IsNullOrWhiteSpace(currentUserId))
            liked = await _dbContext.MovieReviewLikes.AnyAsync(l => l.MovieReviewId == reviewId && l.UserId == currentUserId);

        var likerUserIds = await _dbContext.MovieReviewLikes
            .Where(l => l.MovieReviewId == reviewId)
            .Select(l => l.UserId)
            .Distinct()
            .ToListAsync();

        var displayMap = await GetDisplayNameMapAsync(likerUserIds);
        var likerNames = likerUserIds.Select(id => displayMap.TryGetValue(id, out var name) ? name : "Kullanici").ToList();

        return new ReviewLikeInfoDto
        {
            ReviewId = reviewId,
            LikeCount = likeCount,
            IsLikedByCurrentUser = liked,
            LikerNames = likerNames
        };
    }

    public async Task<List<ReviewReplyAdminDto>> GetPendingRepliesAsync()
    {
        var items = await _dbContext.MovieReviewReplies
            .Include(r => r.MovieReview)
                .ThenInclude(rv => rv.Movie)
            .Where(r => !r.IsApproved)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReviewReplyAdminDto
            {
                Id = r.Id,
                ReviewId = r.MovieReviewId,
                MovieId = r.MovieReview.MovieId,
                MovieTitle = r.MovieReview.Movie.Title,
                UserId = r.UserId,
                DisplayName = "",
                Comment = r.Comment,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToListAsync();

        var userIds = items.Select(i => i.UserId).Distinct().ToList();
        var displayMap = await GetDisplayNameMapAsync(userIds);
        foreach (var item in items)
        {
            if (displayMap.TryGetValue(item.UserId, out var name))
                item.DisplayName = name;
        }

        return items;
    }

    public async Task<List<ReviewReplyAdminDto>> GetRepliesForMovieAsync(int movieId)
    {
        var items = await _dbContext.MovieReviewReplies
            .Include(r => r.MovieReview)
                .ThenInclude(rv => rv.Movie)
            .Where(r => r.MovieReview.MovieId == movieId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ReviewReplyAdminDto
            {
                Id = r.Id,
                ReviewId = r.MovieReviewId,
                MovieId = r.MovieReview.MovieId,
                MovieTitle = r.MovieReview.Movie.Title,
                UserId = r.UserId,
                DisplayName = "",
                Comment = r.Comment,
                CreatedAtUtc = r.CreatedAtUtc
            })
            .ToListAsync();

        var userIds = items.Select(i => i.UserId).Distinct().ToList();
        var displayMap = await GetDisplayNameMapAsync(userIds);
        foreach (var item in items)
        {
            if (displayMap.TryGetValue(item.UserId, out var name))
                item.DisplayName = name;
        }

        return items;
    }

    public async Task<ReviewResultDto> SetReplyApprovalAsync(int replyId, bool isApproved)
    {
        var reply = await _dbContext.MovieReviewReplies.FirstOrDefaultAsync(r => r.Id == replyId);
        if (reply == null)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yanit bulunamadi." } };

        reply.IsApproved = isApproved;
        reply.ApprovedAtUtc = isApproved ? DateTime.UtcNow : null;
        await _dbContext.SaveChangesAsync();
        return new ReviewResultDto { Success = true };
    }

    public async Task<ReviewResultDto> DeleteReplyAsync(int replyId)
    {
        var reply = await _dbContext.MovieReviewReplies.FirstOrDefaultAsync(r => r.Id == replyId);
        if (reply == null)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yanit bulunamadi." } };

        _dbContext.MovieReviewReplies.Remove(reply);
        await _dbContext.SaveChangesAsync();
        return new ReviewResultDto { Success = true };
    }

    public async Task<ReviewResultDto> DeleteReplyForUserAsync(int replyId, string userId, bool isAdmin)
    {
        var reply = await _dbContext.MovieReviewReplies.FirstOrDefaultAsync(r => r.Id == replyId);
        if (reply == null)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Yanit bulunamadi." } };

        if (!isAdmin && reply.UserId != userId)
            return new ReviewResultDto { Success = false, Errors = new List<string> { "Bu yaniti silemezsiniz." } };

        _dbContext.MovieReviewReplies.Remove(reply);
        await _dbContext.SaveChangesAsync();
        return new ReviewResultDto { Success = true };
    }

    private async Task FillDisplayNamesAsync(List<ReviewAdminItemDto> items)
    {
        if (items.Count == 0)
            return;

        var userIds = items.Select(x => x.UserId).Distinct().ToList();
        var profiles = await _dbContext.UserProfiles
            .Where(p => userIds.Contains(p.UserId))
            .Select(p => new
            {
                p.UserId,
                p.UserName,
                FullName = ((p.FirstName ?? "") + " " + (p.LastName ?? "")).Trim()
            })
            .ToListAsync();

        var emails = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToListAsync();

        foreach (var item in items)
        {
            var profile = profiles.FirstOrDefault(p => p.UserId == item.UserId);
            var fullName = profile?.FullName;
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                item.DisplayName = fullName;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(profile?.UserName))
            {
                item.DisplayName = profile.UserName;
                continue;
            }

            var email = emails.FirstOrDefault(u => u.Id == item.UserId)?.Email;
            item.DisplayName = string.IsNullOrWhiteSpace(email) ? "Kullanici" : email;
        }
    }

    private async Task<Dictionary<string, string>> GetDisplayNameMapAsync(List<string> userIds)
    {
        if (userIds.Count == 0)
            return new Dictionary<string, string>();

        var profiles = await _dbContext.UserProfiles
            .Where(p => userIds.Contains(p.UserId))
            .Select(p => new
            {
                p.UserId,
                p.UserName,
                FullName = ((p.FirstName ?? "") + " " + (p.LastName ?? "")).Trim()
            })
            .ToListAsync();

        var emails = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToListAsync();

        var map = new Dictionary<string, string>();
        foreach (var id in userIds)
        {
            var profile = profiles.FirstOrDefault(p => p.UserId == id);
            var fullName = profile?.FullName;
            if (!string.IsNullOrWhiteSpace(fullName))
            {
                map[id] = fullName;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(profile?.UserName))
            {
                map[id] = profile.UserName;
                continue;
            }

            var email = emails.FirstOrDefault(u => u.Id == id)?.Email;
            map[id] = string.IsNullOrWhiteSpace(email) ? "Kullanici" : email;
        }

        return map;
    }
}
