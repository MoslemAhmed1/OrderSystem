using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Auth
{
    public record RefreshTokenRequest
    {
        [Required]
        public required string RefreshToken { get; init; }
    }
}
