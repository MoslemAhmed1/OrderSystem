using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Auth
{
    public record RegisterViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public required string Username { get; init; }

        [Required]
        [EmailAddress]
        public required string Email { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 8)]
        public required string Password { get; init; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string FirstName { get; init; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string LastName { get; init; }
    }
}
