namespace OrderSystem.Infrastructure.Options
{
    public class CachingOptions
    {
        public int DefaultExpirationMinutes { get; set; }
        public int SlidingExpirationMinutes { get; set; }
        public int VersionExpirationDays { get; set; }
    }
}
