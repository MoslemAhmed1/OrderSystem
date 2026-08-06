using OrderSystem.Domain.Enums;

namespace OrderSystem.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        public required string Username { get; set; }

        public required string Email { get; set; }

        public required string PasswordHash { get; set; }

        public UserRole Role { get; set; } = UserRole.Customer;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
