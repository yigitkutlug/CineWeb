using Cinema.Application.Profiles;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Profiles;

public class ProfileService : IProfileService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ProfileService(
        UserManager<IdentityUser> userManager,
        AppDbContext dbContext,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<ProfileEditDto?> GetProfileAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);

        return new ProfileEditDto
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            FirstName = profile?.FirstName,
            LastName = profile?.LastName,
            City = profile?.City,
            Address = profile?.Address,
            ProfilePhotoPath = profile?.ProfilePhotoPath
        };
    }

    public async Task<ProfileUpdateResultDto> UpdateProfileAsync(ProfileUpdateDto dto, ProfilePhotoUploadDto? photo)
    {
        var result = new ProfileUpdateResultDto();

        if (string.IsNullOrWhiteSpace(dto.UserId))
        {
            result.Errors.Add("Kullanici bulunamadi.");
            return result;
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            result.Errors.Add("E-posta zorunludur.");
            return result;
        }

        var user = await _userManager.FindByIdAsync(dto.UserId);
        if (user == null)
        {
            result.Errors.Add("Kullanici bulunamadi.");
            return result;
        }

        user.Email = dto.Email.Trim();
        user.UserName = dto.Email.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

        var identityResult = await _userManager.UpdateAsync(user);
        if (!identityResult.Succeeded)
        {
            foreach (var err in identityResult.Errors)
                result.Errors.Add(err.Description);

            return result;
        }

        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (profile == null)
        {
            profile = new UserProfile { UserId = user.Id };
            _dbContext.UserProfiles.Add(profile);
        }

        profile.FirstName = Normalize(dto.FirstName);
        profile.LastName = Normalize(dto.LastName);
        profile.City = Normalize(dto.City);
        profile.Address = Normalize(dto.Address);

        if (photo != null && photo.Length > 0)
        {
            var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
            {
                result.Errors.Add("Profil fotografi uzantisi sadece .jpg, .jpeg, .png veya .webp olabilir.");
                return result;
            }

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{user.Id}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await photo.Content.CopyToAsync(stream);

            profile.ProfilePhotoPath = $"/uploads/profiles/{fileName}";
        }
        else if (!string.IsNullOrWhiteSpace(dto.ProfilePhotoPath))
        {
            profile.ProfilePhotoPath = dto.ProfilePhotoPath.Trim();
        }

        await _dbContext.SaveChangesAsync();

        result.Success = true;
        return result;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
