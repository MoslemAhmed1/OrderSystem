using Microsoft.AspNetCore.Diagnostics;
using OrderSystem.Common;

namespace OrderSystem.Middlewares
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
                UnauthorizedAccessException e => ((e.Message.Contains("InvalidCredentials") || e.Message.Contains("InvalidRefreshToken")) ? StatusCodes.Status403Forbidden : StatusCodes.Status401Unauthorized, e.Message),
                InvalidOperationException e => (StatusCodes.Status400BadRequest, e.Message),
                KeyNotFoundException e => (StatusCodes.Status404NotFound, e.Message),
                ArgumentException e => (StatusCodes.Status400BadRequest, e.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };
        }
    }
}