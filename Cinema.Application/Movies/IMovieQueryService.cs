using Cinema.Domain.Entities;

namespace Cinema.Application.Movies;

public interface IMovieQueryService
{
    Task<List<Movie>> GetFeaturedMoviesAsync(int maxCount);
}
