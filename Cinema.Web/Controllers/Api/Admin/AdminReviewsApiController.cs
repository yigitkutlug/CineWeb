using Cinema.Application.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Web.Controllers.Api.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminReviewsApiController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public AdminReviewsApiController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<ReviewAdminItemDto>>> GetPending()
    {
        var items = await _reviewService.GetPendingAsync();
        return Ok(items);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<ActionResult> Approve(int id)
    {
        var result = await _reviewService.SetApprovalAsync(id, true);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<ActionResult> Reject(int id)
    {
        var result = await _reviewService.SetApprovalAsync(id, false);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
