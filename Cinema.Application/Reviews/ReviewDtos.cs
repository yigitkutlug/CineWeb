namespace Cinema.Application.Reviews;

public class ReviewListItemDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string UserId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public List<string> LikerNames { get; set; } = new();
    public List<ReviewReplyDto> Replies { get; set; } = new();
}

public class ReviewCreateDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReviewUpdateDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class ReviewResultDto
{
    public bool Success { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ReviewAdminItemDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public string UserId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class ReviewModerationMovieDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public int TotalCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public DateTime? LastReviewAtUtc { get; set; }
    public int ReplyCount { get; set; }
    public int PendingReplyCount { get; set; }
    public DateTime? LastReplyAtUtc { get; set; }
}

public class ReviewSummaryDto
{
    public int ApprovedReviewCount { get; set; }
    public double AverageRating { get; set; }
}

public class ReviewReplyDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public string UserId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Comment { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}

public class ReviewReplyAdminDto
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public string UserId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Comment { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}

public class ReviewLikeInfoDto
{
    public int ReviewId { get; set; }
    public int LikeCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public List<string> LikerNames { get; set; } = new();
}
