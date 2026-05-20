using Cinema.Application.Movies;
using Cinema.Application.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Cinema.Web.ViewModels;

namespace Cinema.Web.Controllers;

// Public film listeleme, detay ve basit bilet alma akisi.
// Not: Bu controller admin CRUD degil; kullanici tarafi "read + satin alma" akisini yonetir.
public class MoviesController : Controller
{
    private readonly IMovieCatalogService _movieCatalogService;
    private readonly ISeatSelectionService _seatSelectionService;
    private readonly ITicketPurchaseService _ticketPurchaseService;
    private readonly IReviewService _reviewService;

    public MoviesController(
        IMovieCatalogService movieCatalogService,
        ISeatSelectionService seatSelectionService,
        ITicketPurchaseService ticketPurchaseService,
        IReviewService reviewService)
    {
        _movieCatalogService = movieCatalogService;
        _seatSelectionService = seatSelectionService;
        _ticketPurchaseService = ticketPurchaseService;
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var movies = await _movieCatalogService.GetAllMoviesAsync();
        return View(movies);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var nowUtc = DateTime.UtcNow;
        var dto = await _movieCatalogService.GetMovieDetailsAsync(id, nowUtc);
        if (dto == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reviews = await _reviewService.GetApprovedForMovieAsync(id, userId);
        var summary = await _reviewService.GetApprovedSummaryAsync(id);
        ReviewListItemDto? myReview = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            if (!string.IsNullOrWhiteSpace(userId))
                myReview = await _reviewService.GetUserReviewAsync(id, userId);
        }

        var vm = new MovieDetailsPageVm
        {
            Details = dto,
            Reviews = reviews,
            ReviewSummary = summary,
            MyReview = myReview,
            ReviewForm = new MovieReviewFormVm
            {
                Rating = myReview?.Rating ?? 5,
                Comment = myReview?.Comment
            }
        };

        return View(vm);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Seats(int showtimeId, string? error = null)
    {
        var nowUtc = DateTime.UtcNow;
        var result = await _seatSelectionService.GetSeatSelectionAsync(showtimeId, nowUtc, error);
        if (result.Data == null)
        {
            return result.MovieId > 0
                ? RedirectToAction(nameof(Details), new { id = result.MovieId })
                : RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Satin alma endpoint'i: koltuk seciminden sonra bilet kaydi olusturur.
    public async Task<IActionResult> BuySeat(int showtimeId, List<int> seatIds)
    {
        var nowUtc = DateTime.UtcNow;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var result = await _ticketPurchaseService.BuySeatsAsync(showtimeId, seatIds, userId, nowUtc);
        if (!result.Success)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return RedirectToAction(nameof(Seats), new { showtimeId, error = result.ErrorMessage });

            return RedirectToAction(nameof(Index));
        }

        TempData["TicketPurchaseSuccess"] = JsonSerializer.Serialize(result.SuccessPayload);
        return RedirectToAction(nameof(PurchaseSuccess));
    }

    [Authorize]
    [HttpGet]
    public IActionResult PurchaseSuccess()
    {
        if (TempData["TicketPurchaseSuccess"] is not string payload)
            return RedirectToAction("Tickets", "Profile");

        var vm = JsonSerializer.Deserialize<TicketPurchaseSuccessDto>(payload);
        if (vm == null)
            return RedirectToAction("Tickets", "Profile");

        TempData.Keep("TicketPurchaseSuccess");
        return View(vm);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id, [Bind(Prefix = "ReviewForm")] MovieReviewFormVm form)
    {
        if (!ModelState.IsValid)
        {
            var nowUtc = DateTime.UtcNow;
            var dto = await _movieCatalogService.GetMovieDetailsAsync(id, nowUtc);
            if (dto == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reviews = await _reviewService.GetApprovedForMovieAsync(id, userId);
            var myReview = string.IsNullOrWhiteSpace(userId)
                ? null
                : await _reviewService.GetUserReviewAsync(id, userId);
            var summary = await _reviewService.GetApprovedSummaryAsync(id);

            var vm = new MovieDetailsPageVm
            {
                Details = dto,
                Reviews = reviews,
                ReviewSummary = summary,
                MyReview = myReview,
                ReviewForm = form
            };

            return View("Details", vm);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
            return Challenge();

        var result = await _reviewService.CreateOrUpdateAsync(id, currentUserId, new ReviewCreateDto
        {
            Rating = form.Rating,
            Comment = form.Comment
        });

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);

            var nowUtc = DateTime.UtcNow;
            var dto = await _movieCatalogService.GetMovieDetailsAsync(id, nowUtc);
            if (dto == null)
                return NotFound();

            var reviews = await _reviewService.GetApprovedForMovieAsync(id, currentUserId);
            var myReview = await _reviewService.GetUserReviewAsync(id, currentUserId);
            var summary = await _reviewService.GetApprovedSummaryAsync(id);

            var vm = new MovieDetailsPageVm
            {
                Details = dto,
                Reviews = reviews,
                ReviewSummary = summary,
                MyReview = myReview,
                ReviewForm = form
            };

            return View("Details", vm);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int reviewId, int movieId, string comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var result = await _reviewService.AddReplyAsync(reviewId, userId, comment);
        if (!result.Success)
            TempData["ReviewError"] = result.Errors.FirstOrDefault() ?? "Islem basarisiz.";

        return RedirectToAction(nameof(Details), new { id = movieId, anchor = "reviews" });
    }

    [Authorize]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ReplyAjax(int reviewId, int movieId, string comment)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _reviewService.AddReplyAsync(reviewId, userId, comment);
        if (!result.Success)
            return BadRequest(new { ok = false, error = result.Errors.FirstOrDefault() ?? "Islem basarisiz." });

        return Json(new { ok = true, pending = true });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int reviewId, int movieId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        await _reviewService.ToggleLikeAsync(reviewId, userId);
        return RedirectToAction(nameof(Details), new { id = movieId, anchor = "reviews" });
    }

    [Authorize]
    [HttpPost]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> LikeAjax(int reviewId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var ok = await _reviewService.ToggleLikeAsync(reviewId, userId);
        if (!ok)
            return BadRequest(new { ok = false });

        var info = await _reviewService.GetLikeInfoAsync(reviewId, userId);
        if (info == null)
            return Json(new { ok = true });

        return Json(new
        {
            ok = true,
            likeCount = info.LikeCount,
            isLiked = info.IsLikedByCurrentUser,
            likerNames = info.LikerNames
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReply(int replyId, int movieId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        var result = await _reviewService.DeleteReplyForUserAsync(replyId, userId, isAdmin);
        if (!result.Success)
            TempData["ReviewError"] = result.Errors.FirstOrDefault() ?? "Islem basarisiz.";

        return RedirectToAction(nameof(Details), new { id = movieId, anchor = "reviews" });
    }
}
