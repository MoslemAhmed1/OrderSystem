using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        public required string Username { get; set; }

        [Required]
        public required string Password { get; set; }

        public string? DeviceInfo { get; set; }
    }
}
