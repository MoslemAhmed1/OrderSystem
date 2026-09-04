using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using OrderSystem.Common;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Auth;

using OrderSystem.Application.DTOs.Auth;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Infrastructure.Options;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITranslationService _translationService;
        private readonly JwtOptions _jwtOptions;

        public AuthController(IAuthService authService, ITranslationService translationService, IOptions<JwtOptions> jwtOptions)
        {
            _authService = authService;
            _translationService = translationService;
            _jwtOptions = jwtOptions.Value;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel request)
        {
            var deviceInfo = GetDeviceInfo();
            var result = await _authService.RegisterAsync(request.ToDto(deviceInfo));

            SetRefreshCookie(result.RefreshToken);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthViewModel>.Success(result.ToViewModel(), _translationService.Translate("UserRegistered"), StatusCodes.Status201Created));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel request)
        {
            var deviceInfo = GetDeviceInfo();
            var result = await _authService.LoginAsync(request.ToDto(deviceInfo));

            SetRefreshCookie(result.RefreshToken);

            return Ok(ApiResponse<AuthViewModel>.Success(result.ToViewModel(), _translationService.Translate("UserLoggedIn")));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(ApiResponse.Fail(_translationService.Translate("RefreshTokenMissing"), StatusCodes.Status401Unauthorized));

            var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });

            SetRefreshCookie(result.RefreshToken);

            return Ok(ApiResponse<AuthViewModel>.Success(result.ToViewModel(), _translationService.Translate("TokenRefreshed")));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _authService.RevokeTokenAsync(refreshToken, GetUserId());

            Response.Cookies.Delete("refreshToken");
            return Ok(ApiResponse.Success(_translationService.Translate("UserLoggedOut")));
        }

        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            await _authService.LogoutAllAsync(GetUserId());
            Response.Cookies.Delete("refreshToken");
            return Ok(ApiResponse.Success(_translationService.Translate("UserLoggedOutAll")));
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel request)
        {
            await _authService.ChangePasswordAsync(request.ToDto(), GetUserId());
            Response.Cookies.Delete("refreshToken");
            return Ok(ApiResponse.Success(_translationService.Translate("PasswordChanged")));
        }

        private void SetRefreshCookie(string token)
        {
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationDays)
            });
        }

        private string GetDeviceInfo()
        {
            return Request.Headers.TryGetValue("User-Agent", out var userAgent) ? userAgent.ToString() : "Unknown";
        }
    }
}
