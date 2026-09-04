using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Auth
{
    public record LoginRequest
    {
        [Required]
        public required string Username { get; init; }

        [Required]
        public required string Password { get; init; }

        public string DeviceInfo { get; init; } = "Unknown";
    }
}
