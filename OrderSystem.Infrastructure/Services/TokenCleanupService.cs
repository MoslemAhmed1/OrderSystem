using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderSystem.Application.Auth;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Services
{
    public class TokenCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TokenCleanupService> _logger;
        private readonly TimeSpan _interval;

        public TokenCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<TokenCleanupService> logger,
            IOptions<JwtOptions> jwtOptions)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _interval = TimeSpan.FromHours(jwtOptions.Value.TokenCleanupIntervalHours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Token cleanup service started");

            await Task.Delay(_interval, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunCleanupAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Token cleanup failed");
                }

                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task RunCleanupAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

            var deleted = await repo.DeleteExpiredAndRevokedAsync();
            _logger.LogInformation("Token cleanup: deleted {Count} expired/revoked refresh token(s)", deleted);
        }
    }
}
