using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Auth
{
    public class RefreshTokenViewModel
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
