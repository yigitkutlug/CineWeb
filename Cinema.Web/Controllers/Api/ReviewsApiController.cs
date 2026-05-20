using Cinema.Application.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cinema.Web.Controllers.Api;

[ApiController]
[Route("api/movies/{movieId:int}/reviews")]
public class ReviewsApiController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsApiController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ReviewListItemDto>>> GetApproved(int movieId)
    {
        var items = await _reviewService.GetApprovedForMovieAsync(movieId, null);
        return Ok(items);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ReviewListItemDto>> GetMine(int movieId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var dto = await _reviewService.GetUserReviewAsync(movieId, userId);
        if (dto == null)
            return NotFound();

        return Ok(dto);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> Create(int movieId, [FromBody] ReviewCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _reviewService.CreateOrUpdateAsync(movieId, userId, dto);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return Ok();
    }

    [HttpPut]
    [Authorize]
    public async Task<ActionResult> Update(int movieId, [FromBody] ReviewUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _reviewService.UpdateAsync(movieId, userId, dto);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> Delete(int movieId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var result = await _reviewService.DeleteAsync(movieId, userId);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
