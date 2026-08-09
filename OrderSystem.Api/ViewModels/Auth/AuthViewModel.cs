namespace OrderSystem.ViewModels.Auth
{
    public class AuthViewModel
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; }
    }
}
