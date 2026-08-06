namespace OrderSystem.Application.Caching
{
    public class CachingOptions
    {
        public int DefaultExpirationMinutes { get; set; } = 10;
        public int SlidingExpirationMinutes { get; set; } = 5;
        public int TokenCleanupIntervalHours { get; set; } = 24;
    }
}
