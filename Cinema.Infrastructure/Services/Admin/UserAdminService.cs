using Cinema.Application.Users;
using Cinema.Infrastructure.Data;
using Cinema.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Infrastructure.Admin;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AppDbContext _dbContext;

    public UserAdminService(UserManager<IdentityUser> userManager, AppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<List<UserListItemDto>> GetUsersAsync()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.Email)
            .ToListAsync();

        var profiles = await _dbContext.UserProfiles
            .AsNoTracking()
            .ToDictionaryAsync(p => p.UserId);

        var items = new List<UserListItemDto>();
        foreach (var user in users)
        {
            profiles.TryGetValue(user.Id, out var profile);
            var roles = await _userManager.GetRolesAsync(user);

            var fullName = string.Join(" ",
                new[] { profile?.FirstName, profile?.LastName }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));

            items.Add(new UserListItemDto
            {
                Id = user.Id,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                FullName = fullName,
                City = profile?.City,
                Roles = roles.OrderBy(r => r).ToList(),
                IsAdmin = roles.Contains("Admin"),
                IsSuperAdmin = roles.Contains("SuperAdmin")
            });
        }

        return items;
    }

    public async Task<UserDetailsDto?> GetUserDetailsAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);

        return new UserDetailsDto
        {
            Id = user.Id,
            Email = user.Email ?? "",
            PhoneNumber = user.PhoneNumber,
            FirstName = profile?.FirstName,
            LastName = profile?.LastName,
            City = profile?.City,
            Address = profile?.Address,
            ProfilePhotoPath = profile?.ProfilePhotoPath,
            Roles = roles.OrderBy(r => r).ToList(),
            IsAdmin = roles.Contains("Admin"),
            IsSuperAdmin = roles.Contains("SuperAdmin")
        };
    }

    public async Task<UserUpdateResultDto> UpdateUserAsync(UserUpdateDto dto)
    {
        var result = new UserUpdateResultDto();

        var user = await _userManager.FindByIdAsync(dto.Id);
        if (user == null)
        {
            result.Errors.Add("Kullanici bulunamadi.");
            return result;
        }

        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                result.Errors.Add(error.Description);

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
        profile.ProfilePhotoPath = Normalize(dto.ProfilePhotoPath);

        await _dbContext.SaveChangesAsync();

        var currentRoles = await _userManager.GetRolesAsync(user);

        if (dto.IsAdmin && !currentRoles.Contains("Admin"))
            await _userManager.AddToRoleAsync(user, "Admin");
        else if (!dto.IsAdmin && currentRoles.Contains("Admin"))
            await _userManager.RemoveFromRoleAsync(user, "Admin");

        if (dto.IsSuperAdmin && !currentRoles.Contains("SuperAdmin"))
            await _userManager.AddToRoleAsync(user, "SuperAdmin");
        else if (!dto.IsSuperAdmin && currentRoles.Contains("SuperAdmin"))
            await _userManager.RemoveFromRoleAsync(user, "SuperAdmin");

        result.Success = true;
        return result;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
