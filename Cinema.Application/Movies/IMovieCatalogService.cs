using Cinema.Domain.Entities;

namespace Cinema.Application.Movies;

// Film listeleme ve detay sorgulari (read-only).
public interface IMovieCatalogService
{
    Task<List<Movie>> GetAllMoviesAsync();
    Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId, DateTime nowUtc);
    Task<List<MovieListItemDto>> GetMovieListForApiAsync();
    Task<MovieDetailsApiDto?> GetMovieDetailsForApiAsync(int movieId, DateTime nowUtc);
}
