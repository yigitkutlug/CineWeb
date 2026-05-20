using Cinema.Application.Movies;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Movies;

public class MovieCatalogService : IMovieCatalogService
{
    private readonly AppDbContext _dbContext;

    public MovieCatalogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Movie>> GetAllMoviesAsync()
    {
        return await _dbContext.Movies
            .OrderByDescending(m => m.IsFeatured)
            .ThenBy(m => m.Title)
            .ToListAsync();
    }

    public async Task<MovieDetailsDto?> GetMovieDetailsAsync(int movieId, DateTime nowUtc)
    {
        var movie = await _dbContext.Movies
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null)
            return null;

        var showtimes = await _dbContext.Showtimes
            .Where(s => s.MovieId == movieId && s.IsActive && s.StartsAt > nowUtc)
            .Include(s => s.Hall)
            .OrderBy(s => s.StartsAt)
            .ToListAsync();

        return new MovieDetailsDto
        {
            Movie = movie,
            Showtimes = showtimes
        };
    }

    public async Task<List<MovieListItemDto>> GetMovieListForApiAsync()
    {
        return await _dbContext.Movies
            .OrderByDescending(m => m.IsFeatured)
            .ThenBy(m => m.Title)
            .Select(m => new MovieListItemDto
            {
                Id = m.Id,
                Title = m.Title,
                Genre = m.Genre,
                DurationMinutes = m.DurationMinutes,
                IsFeatured = m.IsFeatured,
                PosterImagePath = m.PosterImagePath
            })
            .ToListAsync();
    }

    public async Task<MovieDetailsApiDto?> GetMovieDetailsForApiAsync(int movieId, DateTime nowUtc)
    {
        var movie = await _dbContext.Movies
            .FirstOrDefaultAsync(m => m.Id == movieId);

        if (movie == null)
            return null;

        var showtimes = await _dbContext.Showtimes
            .Where(s => s.MovieId == movieId && s.IsActive && s.StartsAt > nowUtc)
            .Include(s => s.Hall)
            .OrderBy(s => s.StartsAt)
            .Select(s => new ShowtimeApiDto
            {
                Id = s.Id,
                StartsAtUtc = s.StartsAt,
                HallName = s.Hall != null ? s.Hall.Name : "Salon",
                Price = s.Price
            })
            .ToListAsync();

        return new MovieDetailsApiDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Genre = movie.Genre,
            DurationMinutes = movie.DurationMinutes,
            Description = movie.Description,
            IsFeatured = movie.IsFeatured,
            PosterImagePath = movie.PosterImagePath,
            Showtimes = showtimes
        };
    }
}
