using System.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Application.DTOs.Auth;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _options;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenHasher _tokenHasher;

        public TokenService(IOptions<JwtOptions> options, IRefreshTokenRepository refreshTokenRepository, ITokenHasher tokenHasher)
        {
            _options = options.Value;
            _refreshTokenRepository = refreshTokenRepository;
            _tokenHasher = tokenHasher;
        }

        public string GenerateAccessToken(User user, DateTime expiresAt)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomNumber);
        }

        public DateTime GetAccessTokenExpiry()
        {
            return DateTime.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes);
        }

        public DateTime GetRefreshTokenExpiry()
        {
            return DateTime.UtcNow.AddDays(_options.RefreshTokenExpirationDays);
        }

        public async Task<AuthResponse> IssueTokensAsync(User user, string deviceInfo)
        {
            var activeToken = await _refreshTokenRepository.GetActiveTokenByUserAndDeviceAsync(user.Id, deviceInfo);
            if (activeToken != null)
            {
                activeToken.RevokedAt = DateTime.UtcNow;
            }

            var accessTokenExpiry = GetAccessTokenExpiry();
            var accessToken = GenerateAccessToken(user, accessTokenExpiry);
            var refreshTokenString = GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                TokenHash = _tokenHasher.Hash(refreshTokenString),
                User = user,
                DeviceInfo = deviceInfo,
                ExpiresAt = GetRefreshTokenExpiry(),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshToken);

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                AccessTokenExpiresAt = accessTokenExpiry
            };
        }
    }
}
