using Microsoft.EntityFrameworkCore;

using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Context;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly OrderContext _orderContext;

        public RefreshTokenRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string tokenHash)
        {
            return await _orderContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task<RefreshToken?> GetActiveTokenByUserAndDeviceAsync(int userId, string deviceInfo)
        {
            var now = DateTime.UtcNow;
            return await _orderContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.DeviceInfo == deviceInfo && rt.RevokedAt == null && rt.ExpiresAt > now);
        }

        public async Task AddAsync(RefreshToken token)
        {
            await _orderContext.RefreshTokens.AddAsync(token);
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            var tokens = await _orderContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public async Task RevokeByUserAndDeviceAsync(int userId, string deviceInfo)
        {
            var tokens = await _orderContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.DeviceInfo == deviceInfo && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }
        }

        public async Task<int> DeleteExpiredAndRevokedAsync()
        {
            var now = DateTime.UtcNow;
            return await _orderContext.RefreshTokens
                .Where(rt => rt.RevokedAt != null || rt.ExpiresAt <= now)
                .ExecuteDeleteAsync();
        }
    }
}
