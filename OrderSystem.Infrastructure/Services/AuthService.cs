using Microsoft.AspNetCore.Identity;
using OrderSystem.Application.DTOs.Auth;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly ITranslationService _translation;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AuthService(
            IUserRepository userRepository, 
            IRefreshTokenRepository refreshTokenRepository, 
            IUnitOfWork uow, 
            ITokenService tokenService, 
            ITranslationService translation)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _uow = uow;
            _tokenService = tokenService;
            _translation = translation;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var (usernameExists, emailExists) = await _userRepository.CheckUserExistsAsync(request.Username, request.Email);
            if (usernameExists)
                throw new InvalidOperationException(_translation.Translate("UsernameExists"));

            if (emailExists)
                throw new InvalidOperationException(_translation.Translate("EmailExists"));

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = "",
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.CreateAsync(user);
            await _uow.CommitAsync();

            var response = await IssueTokensAsync(user, null);
            await _uow.CommitAsync();
            
            return response;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user is null)
                throw new UnauthorizedAccessException(_translation.Translate("InvalidCredentials"));

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException(_translation.Translate("InvalidCredentials"));

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
                _userRepository.Update(user);
            }

            var response = await IssueTokensAsync(user, request.DeviceInfo);
            await _uow.CommitAsync();

            return response;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var hashedToken = _tokenService.HashToken(request.RefreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);
            if (storedToken is null || !storedToken.IsActive)
                throw new UnauthorizedAccessException(_translation.Translate("InvalidRefreshToken"));

            // Revoke old refresh token
            storedToken.RevokedAt = DateTime.UtcNow;

            var response = await IssueTokensAsync(storedToken.User, storedToken.DeviceInfo);
            await _uow.CommitAsync();

            return response;
        }

        public async Task RevokeTokenAsync(string refreshToken)
        {
            var hashedToken = _tokenService.HashToken(refreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);
            if (storedToken is null || !storedToken.IsActive)
                throw new InvalidOperationException(_translation.Translate("TokenInactive"));

            storedToken.RevokedAt = DateTime.UtcNow;
            await _uow.CommitAsync();
        }

        private async Task<AuthResponse> IssueTokensAsync(User user, string? deviceInfo)
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenString = _tokenService.GenerateRefreshToken();
            
            var refreshToken = new RefreshToken
            {
                TokenHash = _tokenService.HashToken(refreshTokenString),
                UserId = user.Id,
                DeviceInfo = deviceInfo,
                ExpiresAt = _tokenService.GetRefreshTokenExpiry(),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.CreateAsync(refreshToken);
            await _uow.CommitAsync();

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenString,
                AccessTokenExpiresAt = _tokenService.GetAccessTokenExpiry()
            };
        }
    }
}
