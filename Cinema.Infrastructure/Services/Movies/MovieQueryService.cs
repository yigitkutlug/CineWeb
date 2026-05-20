using Cinema.Application.Movies;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Movies;

public class MovieQueryService : IMovieQueryService
{
    private readonly AppDbContext _dbContext;

    public MovieQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Movie>> GetFeaturedMoviesAsync(int maxCount)
    {
        return await _dbContext.Movies
            .Where(m => m.IsFeatured)
            .OrderByDescending(m => m.Id)
            .Take(maxCount)
            .ToListAsync();
    }
}
