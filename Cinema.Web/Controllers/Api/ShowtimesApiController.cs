using Cinema.Application.Showtimes;
using Cinema.Application.Movies;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Web.Controllers.Api;

[ApiController]
[Route("api/showtimes")]
public class ShowtimesApiController : ControllerBase
{
    private readonly IShowtimeQueryService _showtimeQueryService;
    private readonly ISeatSelectionService _seatSelectionService;

    public ShowtimesApiController(IShowtimeQueryService showtimeQueryService, ISeatSelectionService seatSelectionService)
    {
        _showtimeQueryService = showtimeQueryService;
        _seatSelectionService = seatSelectionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ShowtimeListDto>>> GetAll()
    {
        var nowUtc = DateTime.UtcNow;
        var items = await _showtimeQueryService.GetShowtimeListForApiAsync(nowUtc);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShowtimeDetailDto>> GetById(int id)
    {
        var nowUtc = DateTime.UtcNow;
        var dto = await _showtimeQueryService.GetShowtimeDetailForApiAsync(id, nowUtc);
        if (dto == null)
            return NotFound();

        return Ok(dto);
    }

    [HttpGet("{id:int}/seats")]
    public async Task<ActionResult<SeatSelectionResultDto>> GetSeatSelection(int id)
    {
        var nowUtc = DateTime.UtcNow;
        var result = await _seatSelectionService.GetSeatSelectionAsync(id, nowUtc, null);
        if (result.Data == null)
            return NotFound();

        if (!string.IsNullOrWhiteSpace(result.Data.ErrorMessage))
            return BadRequest(new { error = result.Data.ErrorMessage });

        return Ok(result);
    }
}
