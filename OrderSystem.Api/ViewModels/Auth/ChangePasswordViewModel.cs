using System.ComponentModel.DataAnnotations;
using OrderSystem.Application.DTOs.Auth;

namespace OrderSystem.ViewModels.Auth
{
    public record ChangePasswordViewModel
    {
        [Required]
        public string CurrentPassword { get; init; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; init; } = string.Empty;

        public ChangePasswordRequest ToDto() => new()
        {
            CurrentPassword = CurrentPassword,
            NewPassword = NewPassword
        };
    }
}
