using OrderSystem.Application.DTOs.Auth;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task RevokeTokenAsync(string refreshToken, int userId);
        Task LogoutAllAsync(int userId);
        Task ChangePasswordAsync(ChangePasswordRequest request, int userId);
    }
}
