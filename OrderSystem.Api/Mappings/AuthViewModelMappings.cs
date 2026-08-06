using OrderSystem.Application.DTOs.Auth;
using OrderSystem.ViewModels.Auth;

namespace OrderSystem.Mappings
{
    public static class AuthViewModelMappings
    {
        public static RegisterRequest ToDto(this RegisterViewModel vm)
        {
            return new RegisterRequest
            {
                Username = vm.Username,
                Email = vm.Email,
                Password = vm.Password,
                FirstName = vm.FirstName,
                LastName = vm.LastName
            };
        }

        public static LoginRequest ToDto(this LoginViewModel vm, string? deviceInfo)
        {
            return new LoginRequest
            {
                Username = vm.Username,
                Password = vm.Password,
                DeviceInfo = deviceInfo
            };
        }

        public static RefreshTokenRequest ToDto(this RefreshTokenViewModel vm)
        {
            return new RefreshTokenRequest { RefreshToken = vm.RefreshToken };
        }

        public static AuthViewModel ToViewModel(this AuthResponse dto)
        {
            return new AuthViewModel
            {
                AccessToken = dto.AccessToken,
                RefreshToken = dto.RefreshToken,
                AccessTokenExpiresAt = dto.AccessTokenExpiresAt
            };
        }
    }
}
