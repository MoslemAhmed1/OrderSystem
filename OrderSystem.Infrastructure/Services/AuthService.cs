using Microsoft.AspNetCore.Identity;
using System.Security.Authentication;

using OrderSystem.Domain.Enums;
using OrderSystem.Domain.Entities;

using OrderSystem.Application.DTOs.Auth;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _uow;
        private readonly ITokenService _tokenService;
        private readonly ITranslationService _translation;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork uow,
            ITokenService tokenService,
            ITranslationService translation)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _customerRepository = customerRepository;
            _uow = uow;
            _tokenService = tokenService;
            _translation = translation;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request) // TODO: should be transaction
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

            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                CustomerType = CustomerType.Regular,
                UserId = user.Id
            };
            await _customerRepository.AddAsync(customer);

            var response = await IssueTokensAsync(user, request.DeviceInfo);
            await _uow.CommitAsync();
            
            return response;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user is null)
                throw new AuthenticationException(_translation.Translate("InvalidCredentials"));

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new AuthenticationException(_translation.Translate("InvalidCredentials"));

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            }

            var response = await IssueTokensAsync(user, request.DeviceInfo);
            await _uow.CommitAsync();

            return response;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var hashedToken = _tokenService.HashToken(request.RefreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);
            
            if (storedToken is null)
                throw new AuthenticationException(_translation.Translate("InvalidRefreshToken"));

            if (storedToken.RevokedAt != null)
            {
                await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId);
                await _uow.CommitAsync();
                throw new AuthenticationException(_translation.Translate("TokenReuseDetected"));
            }

            if (!storedToken.IsActive)
                throw new AuthenticationException(_translation.Translate("InvalidRefreshToken"));

            storedToken.RevokedAt = DateTime.UtcNow;
            
            var response = await IssueTokensAsync(storedToken.User, storedToken.DeviceInfo);
            storedToken.ReplacedByTokenHash = _tokenService.HashToken(response.RefreshToken);
            
            await _uow.CommitAsync();

            return response;
        }

        public async Task RevokeTokenAsync(string refreshToken, int userId)
        {
            var hashedToken = _tokenService.HashToken(refreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);
            
            if (storedToken is null || storedToken.RevokedAt != null || storedToken.UserId != userId)
                return;

            storedToken.RevokedAt = DateTime.UtcNow;
            await _uow.CommitAsync();
        }

        private async Task<AuthResponse> IssueTokensAsync(User user, string? deviceInfo)
        {
            if (!string.IsNullOrEmpty(deviceInfo))
            {
                var activeToken = await _refreshTokenRepository.GetActiveTokenByUserAndDeviceAsync(user.Id, deviceInfo);
                if (activeToken != null)
                {
                    activeToken.RevokedAt = DateTime.UtcNow;
                }
            }

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
