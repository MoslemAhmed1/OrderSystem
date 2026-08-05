using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
