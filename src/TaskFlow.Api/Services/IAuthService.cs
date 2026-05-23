using TaskFlow.Api.Dtos;

namespace TaskFlow.Api.Services;

public interface IAuthService
{
    Task<(bool ok, string error)> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
