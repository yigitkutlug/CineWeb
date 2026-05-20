namespace Cinema.Application.Reviews;

public interface IReviewService
{
    Task<List<ReviewListItemDto>> GetApprovedForMovieAsync(int movieId, string? currentUserId);
    Task<ReviewSummaryDto> GetApprovedSummaryAsync(int movieId);
    Task<ReviewListItemDto?> GetUserReviewAsync(int movieId, string userId);
    Task<ReviewResultDto> CreateOrUpdateAsync(int movieId, string userId, ReviewCreateDto dto);
    Task<ReviewResultDto> UpdateAsync(int movieId, string userId, ReviewUpdateDto dto);
    Task<ReviewResultDto> DeleteAsync(int movieId, string userId);
    Task<ReviewResultDto> AddReplyAsync(int reviewId, string userId, string comment);
    Task<bool> ToggleLikeAsync(int reviewId, string userId);
    Task<ReviewLikeInfoDto?> GetLikeInfoAsync(int reviewId, string? currentUserId);
    Task<ReviewResultDto> DeleteReplyForUserAsync(int replyId, string userId, bool isAdmin);
    Task<List<ReviewReplyAdminDto>> GetRepliesForMovieAsync(int movieId);
    Task<List<ReviewReplyAdminDto>> GetPendingRepliesAsync();
    Task<ReviewResultDto> SetReplyApprovalAsync(int replyId, bool isApproved);
    Task<ReviewResultDto> DeleteReplyAsync(int replyId);

    Task<List<ReviewAdminItemDto>> GetPendingAsync();
    Task<List<ReviewAdminItemDto>> GetAllForMovieAsync(int movieId);
    Task<List<ReviewAdminItemDto>> GetForUserAsync(string userId);
    Task<List<ReviewModerationMovieDto>> GetMoviesForModerationAsync();
    Task<ReviewResultDto> SetApprovalAsync(int reviewId, bool isApproved);
    Task<ReviewResultDto> DeleteByIdAsync(int reviewId);
}
