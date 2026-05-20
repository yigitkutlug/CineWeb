using Cinema.Application.Tickets;
using Cinema.Application.Movies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cinema.Web.Controllers.Api;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsApiController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ITicketPurchaseService _ticketPurchaseService;

    public TicketsApiController(ITicketService ticketService, ITicketPurchaseService ticketPurchaseService)
    {
        _ticketService = ticketService;
        _ticketPurchaseService = ticketPurchaseService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<List<TicketHistoryDto>>> GetMyTickets()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var items = await _ticketService.GetUserTicketsAsync(userId);
        return Ok(items);
    }

    [HttpGet("user/{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<List<AdminTicketItemDto>>> GetUserTickets(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var items = await _ticketService.GetUserTicketsForAdminAsync(id);
        return Ok(items);
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<PurchaseResultDto>> Purchase([FromBody] TicketPurchaseDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (dto.SeatIds == null || dto.SeatIds.Count == 0)
            return BadRequest(new { error = "Koltuk secimi zorunludur." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var nowUtc = DateTime.UtcNow;
        var result = await _ticketPurchaseService.BuySeatsAsync(dto.ShowtimeId, dto.SeatIds, userId, nowUtc);
        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result);
    }
}
