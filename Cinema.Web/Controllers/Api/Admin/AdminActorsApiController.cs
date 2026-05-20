using Cinema.Application.Actors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Web.Controllers.Api.Admin;

[ApiController]
[Route("api/admin/actors")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminActorsApiController : ControllerBase
{
    private readonly IActorAdminService _actorService;

    public AdminActorsApiController(IActorAdminService actorService)
    {
        _actorService = actorService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ActorListItemDto>>> GetAll()
    {
        var actors = await _actorService.GetActorsAsync();
        var items = actors.Select(actor => new ActorListItemDto
        {
            Id = actor.Id,
            FullName = actor.FullName,
            PhotoPath = actor.PhotoPath,
            Age = actor.Age,
            Nationality = actor.Nationality,
            BirthDate = actor.BirthDate
        }).ToList();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ActorDetailDto>> GetById(int id)
    {
        var actor = await _actorService.GetActorAsync(id);
        if (actor == null)
            return NotFound();

        var dto = new ActorDetailDto
        {
            Id = actor.Id,
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            Nationality = actor.Nationality,
            BirthDate = actor.BirthDate,
            PhotoPath = actor.PhotoPath,
            Biography = actor.Biography
        };

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromForm] ActorCreateDto dto, IFormFile? photoFile)
    {
        ActorPhotoUploadDto? photoDto = null;
        if (photoFile != null && photoFile.Length > 0)
        {
            photoDto = new ActorPhotoUploadDto
            {
                FileName = photoFile.FileName,
                Content = photoFile.OpenReadStream(),
                Length = photoFile.Length
            };
        }

        var result = await _actorService.CreateActorAsync(dto, photoDto);

        if (photoDto != null && photoDto.Content != Stream.Null)
            await photoDto.Content.DisposeAsync();

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { id = result.Actor?.Id });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Update(int id, [FromForm] ActorUpdateDto dto, IFormFile? photoFile)
    {
        dto.Id = id;
        ActorPhotoUploadDto? photoDto = null;
        if (photoFile != null && photoFile.Length > 0)
        {
            photoDto = new ActorPhotoUploadDto
            {
                FileName = photoFile.FileName,
                Content = photoFile.OpenReadStream(),
                Length = photoFile.Length
            };
        }

        var result = await _actorService.UpdateActorAsync(dto, photoDto);

        if (photoDto != null && photoDto.Content != Stream.Null)
            await photoDto.Content.DisposeAsync();

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _actorService.DeleteActorAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}
