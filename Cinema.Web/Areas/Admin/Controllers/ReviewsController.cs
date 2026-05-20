using Cinema.Application.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cinema.Web.Areas.Admin.ViewModels;

namespace Cinema.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class ReviewsController : Controller
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _reviewService.GetPendingAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Moderation()
    {
        var items = await _reviewService.GetMoviesForModerationAsync();
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Movie(int id)
    {
        var items = await _reviewService.GetAllForMovieAsync(id);
        if (items.Count == 0)
            return RedirectToAction(nameof(Moderation));

        var pendingReplies = await _reviewService.GetPendingRepliesAsync();
        var allReplies = await _reviewService.GetRepliesForMovieAsync(id);
        var vm = new ReviewModerationVm
        {
            MovieId = id,
            MovieTitle = items.First().MovieTitle,
            Reviews = items,
            PendingReplies = pendingReplies.Where(r => r.MovieId == id).ToList(),
            AllReplies = allReplies
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, int? movieId)
    {
        var result = await _reviewService.SetApprovalAsync(id, true);
        if (!result.Success)
            return movieId.HasValue ? RedirectToAction(nameof(Movie), new { id = movieId }) : RedirectToAction(nameof(Index));

        return movieId.HasValue ? RedirectToAction(nameof(Movie), new { id = movieId }) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, int? movieId)
    {
        var result = await _reviewService.SetApprovalAsync(id, false);
        if (!result.Success)
            return movieId.HasValue ? RedirectToAction(nameof(Movie), new { id = movieId }) : RedirectToAction(nameof(Index));

        return movieId.HasValue ? RedirectToAction(nameof(Movie), new { id = movieId }) : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int movieId)
    {
        var result = await _reviewService.DeleteByIdAsync(id);
        if (!result.Success)
            return RedirectToAction(nameof(Movie), new { id = movieId });

        return RedirectToAction(nameof(Movie), new { id = movieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplyApprove(int id, int movieId)
    {
        await _reviewService.SetReplyApprovalAsync(id, true);
        return RedirectToAction(nameof(Movie), new { id = movieId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplyReject(int id, int movieId)
    {
        await _reviewService.DeleteReplyAsync(id);
        return RedirectToAction(nameof(Movie), new { id = movieId });
    }
}
