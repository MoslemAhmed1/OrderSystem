namespace OrderSystem.Middlewares
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static int _counter = 0;
        private static DateTime _lastRequestDate = DateTime.Now;

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _counter++;
            int secondsDifference = DateTime.Now.Subtract(_lastRequestDate).Seconds;
            _lastRequestDate = DateTime.Now;

            if (secondsDifference > 10)
            {
                _counter = 1;
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
