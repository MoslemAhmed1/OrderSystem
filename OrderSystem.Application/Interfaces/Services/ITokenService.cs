using OrderSystem.Domain.Entities;
using OrderSystem.Application.DTOs.Auth;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, DateTime expiresAt);
        string GenerateRefreshToken();
        DateTime GetAccessTokenExpiry();
        DateTime GetRefreshTokenExpiry();
        Task<AuthResponse> IssueTokensAsync(User user, string deviceInfo);
    }
}

