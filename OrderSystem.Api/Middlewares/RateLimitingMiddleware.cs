namespace OrderSystem.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static int _counter = 0;
        private static DateTime _lastRequestDate = DateTime.UtcNow;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            Interlocked.Increment(ref _counter);
            double secondsDifference = DateTime.UtcNow.Subtract(_lastRequestDate).TotalSeconds;
            _lastRequestDate = DateTime.UtcNow;

            if (secondsDifference > 10)
            {
                Interlocked.Exchange(ref _counter, 1);
                await _next(context);
            }
            else
            {
                if (_counter > 5)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.Response.WriteAsync("Rate Limit Exceeded!");
                }
                else
                {
                    await _next(context);
                }
            }
        }
    }

    public static class RateLimitingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RateLimitingMiddleware>();
        }
    }
}
