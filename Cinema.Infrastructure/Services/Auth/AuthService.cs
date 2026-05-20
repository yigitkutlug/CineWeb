using Cinema.Application.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Cinema.Infrastructure.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
        {
            return new AuthResultDto
            {
                Success = false,
                Errors = new List<string> { "Bu e-posta zaten kayitli." }
            };
        }

        var user = new IdentityUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return new AuthResultDto
            {
                Success = false,
                Errors = result.Errors.Select(e => e.Description).ToList()
            };
        }

        await TryAssignDefaultRoleAsync(user);
        return await CreateTokenAsync(user);
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return new AuthResultDto
            {
                Success = false,
                Errors = new List<string> { "Gecersiz e-posta veya sifre." }
            };
        }

        var ok = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!ok)
        {
            return new AuthResultDto
            {
                Success = false,
                Errors = new List<string> { "Gecersiz e-posta veya sifre." }
            };
        }

        return await CreateTokenAsync(user);
    }

    private async Task<AuthResultDto> CreateTokenAsync(IdentityUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "DEV_ONLY_CHANGE_ME";
        var issuer = _configuration["Jwt:Issuer"] ?? "Cinema.Web";
        var audience = _configuration["Jwt:Audience"] ?? "Cinema.Web";
        var expiryMinutes = 60;
        if (int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var minutes) && minutes > 0)
            expiryMinutes = minutes;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? "")
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResultDto
        {
            Success = true,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expires
        };
    }

    private async Task TryAssignDefaultRoleAsync(IdentityUser user)
    {
        if (await _roleManager.RoleExistsAsync("Kullanici"))
        {
            await _userManager.AddToRoleAsync(user, "Kullanici");
            return;
        }

        if (await _roleManager.RoleExistsAsync("User"))
            await _userManager.AddToRoleAsync(user, "User");
    }
}
