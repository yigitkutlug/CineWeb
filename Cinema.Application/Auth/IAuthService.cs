namespace Cinema.Application.Auth;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto dto);
    Task<AuthResultDto> LoginAsync(LoginRequestDto dto);
}
