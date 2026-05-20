using Cinema.Application.AdminMovies;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

namespace Cinema.Infrastructure.Admin;

public class AdminMovieService : IAdminMovieService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public AdminMovieService(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<List<AdminMovieListDto>> GetMoviesAsync()
    {
        return await _dbContext.Movies
            .Include(m => m.MovieActors)
            .OrderBy(m => m.Title)
            .Select(m => new AdminMovieListDto
            {
                Id = m.Id,
                Title = m.Title,
                Genre = m.Genre,
                DurationMinutes = m.DurationMinutes,
                ActorCount = m.MovieActors.Count,
                IsFeatured = m.IsFeatured
            })
            .ToListAsync();
    }

    public async Task<List<ActorOptionDto>> GetActorOptionsAsync()
    {
        return await _dbContext.Actors
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .Select(a => new ActorOptionDto
            {
                Id = a.Id,
                Name = string.IsNullOrWhiteSpace(a.FullName) ? $"{a.FirstName} {a.LastName}" : a.FullName
            })
            .ToListAsync();
    }

    public async Task<AdminMovieEditDto?> GetMovieForEditAsync(int id)
    {
        var movie = await _dbContext.Movies
            .Include(m => m.MovieActors)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
            return null;

        return new AdminMovieEditDto
        {
            Id = movie.Id,
            Title = movie.Title,
            DurationMinutes = movie.DurationMinutes,
            Genre = movie.Genre,
            Description = movie.Description,
            PosterImagePath = movie.PosterImagePath,
            IsFeatured = movie.IsFeatured,
            SelectedActorIds = movie.MovieActors.Select(x => x.ActorId).ToList()
        };
    }

    public async Task<MovieUpsertResultDto> CreateMovieAsync(MovieUpsertDto dto, PosterUploadDto? poster)
    {
        var result = ValidateMovieUpsert(dto);
        if (!result.Success)
            return result;

        string? posterImagePath = null;
        if (poster != null && poster.Length > 0)
        {
            var posterPath = await SavePosterPathAsync(poster);
            if (posterPath == null)
            {
                result.Errors.Add("Film kapağı sadece .jpg, .jpeg, .png veya .webp olabilir.");
                result.Success = false;
                return result;
            }

            posterImagePath = posterPath;
        }

        var model = new Movie
        {
            Title = dto.Title,
            DurationMinutes = dto.DurationMinutes,
            Genre = dto.Genre ?? "",
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            PosterImagePath = posterImagePath,
            IsFeatured = dto.IsFeatured
        };

        _dbContext.Movies.Add(model);
        await _dbContext.SaveChangesAsync();

        foreach (var actorId in dto.SelectedActorIds.Distinct())
        {
            _dbContext.MovieActors.Add(new MovieActor { MovieId = model.Id, ActorId = actorId });
        }

        await _dbContext.SaveChangesAsync();

        result.Success = true;
        result.MovieId = model.Id;
        return result;
    }

    public async Task<MovieUpsertResultDto> UpdateMovieAsync(MovieUpsertDto dto, PosterUploadDto? poster)
    {
        var result = ValidateMovieUpsert(dto);
        if (!result.Success)
            return result;

        var movie = await _dbContext.Movies
            .Include(m => m.MovieActors)
            .FirstOrDefaultAsync(m => m.Id == dto.Id);
        if (movie == null)
        {
            result.Errors.Add("Film bulunamadi.");
            result.Success = false;
            return result;
        }

        var posterImagePath = dto.PosterImagePath;
        if (poster != null && poster.Length > 0)
        {
            var posterPath = await SavePosterPathAsync(poster);
            if (posterPath == null)
            {
                result.Errors.Add("Film kapağı sadece .jpg, .jpeg, .png veya .webp olabilir.");
                result.Success = false;
                return result;
            }

            posterImagePath = posterPath;
        }

        movie.Title = dto.Title;
        movie.DurationMinutes = dto.DurationMinutes;
        movie.Genre = dto.Genre ?? "";
        movie.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        movie.PosterImagePath = posterImagePath;
        movie.IsFeatured = dto.IsFeatured;

        var selectedIds = dto.SelectedActorIds.Distinct().ToHashSet();
        var existingIds = movie.MovieActors.Select(x => x.ActorId).ToHashSet();

        var toRemove = movie.MovieActors.Where(x => !selectedIds.Contains(x.ActorId)).ToList();
        if (toRemove.Count > 0)
            _dbContext.MovieActors.RemoveRange(toRemove);

        foreach (var actorId in selectedIds.Where(id => !existingIds.Contains(id)))
        {
            _dbContext.MovieActors.Add(new MovieActor { MovieId = movie.Id, ActorId = actorId });
        }

        await _dbContext.SaveChangesAsync();

        result.Success = true;
        result.MovieId = movie.Id;
        return result;
    }

    public async Task<bool> DeleteMovieAsync(int id)
    {
        var movie = await _dbContext.Movies.FindAsync(id);
        if (movie == null)
            return false;

        // Bilet gecmisi bozulmasin diye MovieId/ShowtimeId baglarini kopar.
        var showtimeIds = await _dbContext.Showtimes
            .Where(s => s.MovieId == id)
            .Select(s => s.Id)
            .ToListAsync();

        var tickets = await _dbContext.Tickets
            .Where(t => t.MovieId == id || (t.ShowtimeId.HasValue && showtimeIds.Contains(t.ShowtimeId.Value)))
            .ToListAsync();

        foreach (var ticket in tickets)
        {
            if (ticket.MovieId == id)
                ticket.MovieId = null;

            if (ticket.ShowtimeId.HasValue && showtimeIds.Contains(ticket.ShowtimeId.Value))
                ticket.ShowtimeId = null;
        }

        _dbContext.Movies.Remove(movie);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private MovieUpsertResultDto ValidateMovieUpsert(MovieUpsertDto dto)
    {
        var result = new MovieUpsertResultDto();
        if (dto.SelectedActorIds == null || dto.SelectedActorIds.Count == 0)
        {
            result.Errors.Add("Film olustururken en az 1 aktor secmek zorunludur.");
            result.Success = false;
            return result;
        }

        result.Success = true;
        return result;
    }

    private async Task<string?> SavePosterPathAsync(PosterUploadDto poster)
    {
        var ext = Path.GetExtension(poster.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext))
            return null;

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "movies");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await poster.Content.CopyToAsync(stream);

        return $"/uploads/movies/{fileName}";
    }
}
