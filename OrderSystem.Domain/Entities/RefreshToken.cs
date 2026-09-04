namespace OrderSystem.Domain.Entities;

public class RefreshToken
{
    public int Id { get; set; }

    public required string TokenHash { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string DeviceInfo { get; set; } = "Unknown";

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public bool IsExpired => ExpiresAt <= DateTime.UtcNow;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
