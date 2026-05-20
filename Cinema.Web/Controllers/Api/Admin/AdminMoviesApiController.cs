using Cinema.Application.AdminMovies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Web.Controllers.Api.Admin;

[ApiController]
[Route("api/admin/movies")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminMoviesApiController : ControllerBase
{
    private readonly IAdminMovieService _movieService;

    public AdminMoviesApiController(IAdminMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminMovieListDto>>> GetAll()
    {
        var items = await _movieService.GetMoviesAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AdminMovieEditDto>> GetById(int id)
    {
        var dto = await _movieService.GetMovieForEditAsync(id);
        if (dto == null)
            return NotFound();

        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<MovieUpsertResultDto>> Create([FromForm] MovieUpsertDto dto, IFormFile? posterFile)
    {
        PosterUploadDto? posterDto = null;
        if (posterFile != null && posterFile.Length > 0)
        {
            posterDto = new PosterUploadDto
            {
                FileName = posterFile.FileName,
                Content = posterFile.OpenReadStream(),
                Length = posterFile.Length
            };
        }

        var result = await _movieService.CreateMovieAsync(dto, posterDto);

        if (posterDto != null && posterDto.Content != Stream.Null)
            await posterDto.Content.DisposeAsync();

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MovieUpsertResultDto>> Update(int id, [FromForm] MovieUpsertDto dto, IFormFile? posterFile)
    {
        dto.Id = id;
        PosterUploadDto? posterDto = null;
        if (posterFile != null && posterFile.Length > 0)
        {
            posterDto = new PosterUploadDto
            {
                FileName = posterFile.FileName,
                Content = posterFile.OpenReadStream(),
                Length = posterFile.Length
            };
        }

        var result = await _movieService.UpdateMovieAsync(dto, posterDto);

        if (posterDto != null && posterDto.Content != Stream.Null)
            await posterDto.Content.DisposeAsync();

        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _movieService.DeleteMovieAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("actor-options")]
    public async Task<ActionResult<List<ActorOptionDto>>> GetActorOptions()
    {
        var items = await _movieService.GetActorOptionsAsync();
        return Ok(items);
    }
}
