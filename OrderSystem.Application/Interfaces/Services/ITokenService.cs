using OrderSystem.Domain.Entities;
using System.Security.Claims;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        string HashToken(string token);
        DateTime GetAccessTokenExpiry();
        DateTime GetRefreshTokenExpiry();
    }
}
