namespace OrderSystem.Application.DTOs.Auth
{
    public record AuthResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; init; }
    }
}
