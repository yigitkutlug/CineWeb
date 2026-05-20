using Cinema.Application.Movies;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Web.Controllers.Api;

[ApiController]
[Route("api/movies")]
public class MoviesApiController : ControllerBase
{
    private readonly IMovieCatalogService _movieCatalogService;

    public MoviesApiController(IMovieCatalogService movieCatalogService)
    {
        _movieCatalogService = movieCatalogService;
    }

    [HttpGet]
    public async Task<ActionResult<List<MovieListItemDto>>> GetAll()
    {
        var items = await _movieCatalogService.GetMovieListForApiAsync();
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovieDetailsApiDto>> GetById(int id)
    {
        var nowUtc = DateTime.UtcNow;
        var dto = await _movieCatalogService.GetMovieDetailsForApiAsync(id, nowUtc);
        if (dto == null)
            return NotFound();

        return Ok(dto);
    }
}
