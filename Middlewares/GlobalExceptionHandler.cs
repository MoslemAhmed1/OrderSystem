using Microsoft.AspNetCore.Diagnostics;
using OrderSystem.Common;

namespace OrderSystem.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var (statusCode, message) = MapException(exception);

            if (statusCode == StatusCodes.Status500InternalServerError)
                _logger.LogError(exception, "Unhandled exception occurred");
            else
                _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

            httpContext.Response.StatusCode = statusCode;

            var response = ApiResponse.Fail(message, statusCode);

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Message) MapException(Exception exception)
        {
            return exception switch
            {
                InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message),
                KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
                ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };
        }
    }
}