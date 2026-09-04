namespace OrderSystem.ViewModels.Auth
{
    public record AuthViewModel
    {
        public string AccessToken { get; init; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; init; }
    }
}
