namespace OrderSystem.Infrastructure.Options
{
    public class AppLocalizationOptions
    {
        public string DefaultCulture { get; set; } = null!;
        public string[] SupportedCultures { get; set; } = null!;
    }
}
