using Cinema.Application.Profiles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cinema.Web.Controllers.Api;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileApiController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileApiController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileEditDto>> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var profile = await _profileService.GetProfileAsync(userId);
        if (profile == null)
            return NotFound();

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult> UpdateMe([FromBody] ProfileUpdateDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        dto.UserId = userId;
        var result = await _profileService.UpdateProfileAsync(dto, null);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
