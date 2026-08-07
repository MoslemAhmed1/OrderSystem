using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel request)
        {
            var deviceInfo = GetDeviceInfo();
            var result = await _authService.RegisterAsync(request.ToDto(deviceInfo));

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthViewModel>.Success(result.ToViewModel(), "User registered successfully.", StatusCodes.Status201Created));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel request)
        {
            var deviceInfo = GetDeviceInfo();
            var result = await _authService.LoginAsync(request.ToDto(deviceInfo));

            return Ok(ApiResponse<AuthViewModel>.Success(result.ToViewModel(), "User logged in successfully."));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshTokenViewModel request)
        {
            var result = await _authService.RefreshTokenAsync(request.ToDto());

            return Ok(ApiResponse<AuthViewModel>.Success(result.ToViewModel(), "Token refreshed successfully."));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(RefreshTokenViewModel request)
        {
            await _authService.RevokeTokenAsync(request.ToDto().RefreshToken, GetUserId());
            return Ok(ApiResponse.Success("User logged out successfully."));
        }

        private string GetDeviceInfo()
        {
            return Request.Headers.TryGetValue("User-Agent", out var userAgent) ? userAgent.ToString() : "Unknown Device";
        }
    }
}
