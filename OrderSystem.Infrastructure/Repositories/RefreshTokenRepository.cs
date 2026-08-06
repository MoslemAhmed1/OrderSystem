using Microsoft.EntityFrameworkCore;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Data;

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

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
        {
            var now = DateTime.UtcNow;
            return await _orderContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
                .ToListAsync();
        }

        public async Task CreateAsync(RefreshToken token)
        {
            await _orderContext.RefreshTokens.AddAsync(token);
        }

        public async Task<int> RevokeAllForUserAsync(int userId)
        {
            var tokens = await _orderContext.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            return tokens.Count;
        }

        public async Task<int> DeleteExpiredAndRevokedAsync()
        {
            var now = DateTime.UtcNow;
            return await _orderContext.RefreshTokens
                .Where(rt => !rt.IsActive)
                .ExecuteDeleteAsync();
        }
    }
}
