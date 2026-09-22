using ProjetFullstack.DTO.Auth;

namespace ProjetFullstack.Services.Interfaces;

public interface IAuthService
{
Task<AuthResponse> RegisterAsync(
RegisterRequest request);

Task<AuthResponse> LoginAsync(
    LoginRequest request);

Task<AuthResponse> RefreshTokenAsync(
    string refreshToken);

Task RevokeRefreshTokenAsync(
    string refreshToken);

}
