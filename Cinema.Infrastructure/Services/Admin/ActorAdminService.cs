using Cinema.Application.Actors;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Admin;

public class ActorAdminService : IActorAdminService
{
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ActorAdminService(AppDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<List<Actor>> GetActorsAsync()
    {
        return await _dbContext.Actors
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .ToListAsync();
    }

    public async Task<Actor?> GetActorAsync(int id)
    {
        return await _dbContext.Actors.FindAsync(id);
    }

    public async Task<ActorCreateResultDto> CreateActorAsync(ActorCreateDto dto, ActorPhotoUploadDto? photo)
    {
        var result = new ActorCreateResultDto();
        var actor = new Actor
        {
            FirstName = dto.FirstName?.Trim() ?? "",
            LastName = dto.LastName?.Trim() ?? "",
            FullName = $"{dto.FirstName} {dto.LastName}".Trim(),
            Nationality = Normalize(dto.Nationality),
            BirthDate = NormalizeBirthDate(dto.BirthDate),
            Biography = Normalize(dto.Biography)
        };

        if (string.IsNullOrWhiteSpace(actor.FirstName) || string.IsNullOrWhiteSpace(actor.LastName))
        {
            result.Errors.Add("Ad ve soyad zorunludur.");
            result.Success = false;
            return result;
        }

        if (photo != null && photo.Length > 0)
        {
            var path = await SaveActorPhotoAsync(photo);
            if (path == null)
            {
                result.Errors.Add("Sadece .jpg, .jpeg, .png veya .webp dosyalari yukleyebilirsin.");
                result.Success = false;
                return result;
            }

            actor.PhotoPath = path;
        }

        _dbContext.Actors.Add(actor);
        await _dbContext.SaveChangesAsync();

        result.Success = true;
        result.Actor = actor;
        return result;
    }

    public async Task<ActorUpdateResultDto> UpdateActorAsync(ActorUpdateDto dto, ActorPhotoUploadDto? photo)
    {
        var result = new ActorUpdateResultDto();
        var actor = await _dbContext.Actors.FindAsync(dto.Id);
        if (actor == null)
        {
            result.Errors.Add("Oyuncu bulunamadi.");
            result.Success = false;
            return result;
        }

        actor.FirstName = dto.FirstName?.Trim() ?? "";
        actor.LastName = dto.LastName?.Trim() ?? "";
        actor.FullName = $"{actor.FirstName} {actor.LastName}".Trim();
        actor.Nationality = Normalize(dto.Nationality);
        actor.BirthDate = NormalizeBirthDate(dto.BirthDate);
        actor.Biography = Normalize(dto.Biography);

        if (photo != null && photo.Length > 0)
        {
            var path = await SaveActorPhotoAsync(photo);
            if (path == null)
            {
                result.Errors.Add("Sadece .jpg, .jpeg, .png veya .webp dosyalari yukleyebilirsin.");
                result.Success = false;
                return result;
            }

            actor.PhotoPath = path;
        }
        else if (!string.IsNullOrWhiteSpace(dto.PhotoPath))
        {
            actor.PhotoPath = dto.PhotoPath.Trim();
        }

        await _dbContext.SaveChangesAsync();

        result.Success = true;
        result.Actor = actor;
        return result;
    }

    public async Task<bool> DeleteActorAsync(int id)
    {
        var actor = await _dbContext.Actors.FindAsync(id);
        if (actor == null)
            return false;

        _dbContext.Actors.Remove(actor);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static DateTime? NormalizeBirthDate(DateTime? value)
    {
        if (!value.HasValue)
            return null;

        var d = value.Value.Date;
        return DateTime.SpecifyKind(d, DateTimeKind.Utc);
    }

    private async Task<string?> SaveActorPhotoAsync(ActorPhotoUploadDto file)
    {
        if (file.Length <= 0)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(extension))
            return null;

        var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "actors");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadsRoot, fileName);

        await using var stream = System.IO.File.Create(fullPath);
        await file.Content.CopyToAsync(stream);

        return $"/uploads/actors/{fileName}";
    }
}
