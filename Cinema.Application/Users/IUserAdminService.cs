namespace Cinema.Application.Users;

public interface IUserAdminService
{
    Task<List<UserListItemDto>> GetUsersAsync();
    Task<UserDetailsDto?> GetUserDetailsAsync(string id);
    Task<UserUpdateResultDto> UpdateUserAsync(UserUpdateDto dto);
}
