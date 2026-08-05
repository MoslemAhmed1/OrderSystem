using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string tokenHash);
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(int userId);
        Task CreateAsync(RefreshToken token);
        Task UpdateAsync(RefreshToken token);
        Task<int> RevokeAllForUserAsync(int userId);
    }
}
