using Cinema.Application.Reviews;

namespace Cinema.Web.Areas.Admin.ViewModels;

public class ReviewModerationVm
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = "";
    public List<ReviewAdminItemDto> Reviews { get; set; } = new();
    public List<ReviewReplyAdminDto> PendingReplies { get; set; } = new();
    public List<ReviewReplyAdminDto> AllReplies { get; set; } = new();
}
