using Cinema.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinema.Web.Controllers.Api.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminUsersApiController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public AdminUsersApiController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserListItemDto>>> GetAll()
    {
        var items = await _userAdminService.GetUsersAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailsDto>> GetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return NotFound();

        var dto = await _userAdminService.GetUserDetailsAsync(id);
        if (dto == null)
            return NotFound();

        return Ok(dto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(string id, [FromBody] UserUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        dto.Id = id;
        var result = await _userAdminService.UpdateUserAsync(dto);
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });

        return NoContent();
    }
}
