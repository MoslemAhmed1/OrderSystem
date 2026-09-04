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
        private readonly ITokenHasher _tokenHasher;
        private readonly ITranslationService _translation;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ICustomerRepository customerRepository,
            IUnitOfWork uow,
            ITokenService tokenService,
            ITokenHasher tokenHasher,
            ITranslationService translation,
            IPasswordHasher<User> passwordHasher)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _customerRepository = customerRepository;
            _uow = uow;
            _tokenService = tokenService;
            _tokenHasher = tokenHasher;
            _translation = translation;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var (usernameExists, emailExists) = await _userRepository.CheckUserExistsAsync(request.Username, request.Email);
            if (usernameExists)
                throw new InvalidOperationException(_translation.Translate("UsernameExists"));
            if (emailExists)
                throw new InvalidOperationException(_translation.Translate("EmailExists"));

            await _uow.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    PasswordHash = "",
                };
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
                await _userRepository.AddAsync(user);

                var customer = new Customer
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CustomerType = CustomerType.Regular,
                    User = user
                };
                await _customerRepository.AddAsync(customer);

                var response = await _tokenService.IssueTokensAsync(user, request.DeviceInfo);
                await _uow.CommitTransactionAsync();

                return response;
            }
            catch
            {
                await _uow.RollbackTransactionAsync();
                throw;
            }
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

            var response = await _tokenService.IssueTokensAsync(user, request.DeviceInfo);
            await _uow.SaveChangesAsync();

            return response;
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var hashedToken = _tokenHasher.Hash(request.RefreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

            if (storedToken is null)
                throw new AuthenticationException(_translation.Translate("InvalidRefreshToken"));

            if (storedToken.RevokedAt != null) // Token reuse, revoke all
            {
                await _refreshTokenRepository.RevokeAllForUserAsync(storedToken.UserId);
                await _uow.SaveChangesAsync();
                throw new AuthenticationException(_translation.Translate("TokenReuseDetected"));
            }

            if (!storedToken.IsActive)
                throw new AuthenticationException(_translation.Translate("RefreshTokenExpired"));

            storedToken.RevokedAt = DateTime.UtcNow;

            var response = await _tokenService.IssueTokensAsync(storedToken.User, storedToken.DeviceInfo);

            await _uow.SaveChangesAsync();

            return response;
        }

        public async Task RevokeTokenAsync(string refreshToken, int userId)
        {
            var hashedToken = _tokenHasher.Hash(refreshToken);
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(hashedToken);

            if (storedToken is null || storedToken.RevokedAt != null || storedToken.UserId != userId)
                return;

            await _refreshTokenRepository.RevokeByUserAndDeviceAsync(userId, storedToken.DeviceInfo);
            await _uow.SaveChangesAsync();
        }

        public async Task LogoutAllAsync(int userId)
        {
            await _refreshTokenRepository.RevokeAllForUserAsync(userId);
            await _uow.SaveChangesAsync();
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest request, int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new KeyNotFoundException(_translation.Translate("UserNotFound"));

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (result == PasswordVerificationResult.Failed)
                throw new AuthenticationException(_translation.Translate("InvalidCredentials"));

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            _userRepository.Update(user);

            await _refreshTokenRepository.RevokeAllForUserAsync(userId);

            await _uow.SaveChangesAsync();
        }
    }
}
