using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Application.DTOs.Auth;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Common;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Auth;
using System.Security.Claims;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel request)
        {
            var deviceInfo = GetDeviceInfo();
            var result = await _authService.RegisterAsync(request.ToDto(deviceInfo));

            SetRefreshCookie(result.RefreshToken);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthViewModel>.Success(result.ToViewModel(), "User registered successfully.", StatusCodes.Status201Created));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel request)
        {
            var deviceInfo = GetDeviceInfo();
            var result = await _authService.LoginAsync(request.ToDto(deviceInfo));

            SetRefreshCookie(result.RefreshToken);

            return Ok(ApiResponse<AuthViewModel>.Success(result.ToViewModel(), "User logged in successfully."));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(ApiResponse.Fail("Refresh token is missing.", StatusCodes.Status401Unauthorized));

            var result = await _authService.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = refreshToken });

            SetRefreshCookie(result.RefreshToken);

            return Ok(ApiResponse<AuthViewModel>.Success(result.ToViewModel(), "Token refreshed successfully."));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _authService.RevokeTokenAsync(refreshToken, GetUserId());

            Response.Cookies.Delete("refreshToken");
            return Ok(ApiResponse.Success("User logged out successfully."));
        }

        private void SetRefreshCookie(string token)
        {
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        }

        private string GetDeviceInfo()
        {
            return Request.Headers.TryGetValue("User-Agent", out var userAgent) ? userAgent.ToString() : "Unknown Device";
        }
    }
}
