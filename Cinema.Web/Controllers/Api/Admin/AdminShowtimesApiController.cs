using Cinema.Application.Showtimes;
using Cinema.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Web.Controllers.Api.Admin;

[ApiController]
[Route("api/admin/showtimes")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminShowtimesApiController : ControllerBase
{
    private readonly IShowtimeAdminService _showtimeAdminService;

    public AdminShowtimesApiController(IShowtimeAdminService showtimeAdminService)
    {
        _showtimeAdminService = showtimeAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ShowtimeAdminItemDto>>> GetAll()
    {
        var items = await _showtimeAdminService.GetShowtimesAsync(null);
        var result = items.Select(showtime => new ShowtimeAdminItemDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            HallId = showtime.HallId,
            StartsAtUtc = showtime.StartsAt,
            Price = showtime.Price,
            IsActive = showtime.IsActive
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShowtimeAdminItemDto>> GetById(int id)
    {
        var showtime = await _showtimeAdminService.GetShowtimeAsync(id);
        if (showtime == null)
            return NotFound();

        var dto = new ShowtimeAdminItemDto
        {
            Id = showtime.Id,
            MovieId = showtime.MovieId,
            HallId = showtime.HallId,
            StartsAtUtc = showtime.StartsAt,
            Price = showtime.Price,
            IsActive = showtime.IsActive
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] ShowtimeAdminUpsertDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var entity = new Showtime
        {
            MovieId = dto.MovieId,
            HallId = dto.HallId,
            StartsAt = DateTime.SpecifyKind(dto.StartsAtUtc, DateTimeKind.Utc),
            Price = dto.Price,
            IsActive = dto.IsActive
        };

        var id = await _showtimeAdminService.CreateShowtimeAsync(entity);
        return Ok(new { id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromBody] ShowtimeAdminUpsertDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var entity = await _showtimeAdminService.GetShowtimeAsync(id);
        if (entity == null)
            return NotFound();

        entity.MovieId = dto.MovieId;
        entity.HallId = dto.HallId;
        entity.StartsAt = DateTime.SpecifyKind(dto.StartsAtUtc, DateTimeKind.Utc);
        entity.Price = dto.Price;
        entity.IsActive = dto.IsActive;

        await _showtimeAdminService.UpdateShowtimeAsync(entity);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _showtimeAdminService.DeleteShowtimeAsync(id);
        return NoContent();
    }
}
