using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Auth
{
    public record LoginViewModel
    {
        [Required]
        public required string Username { get; init; }

        [Required]
        public required string Password { get; init; }
    }
}
