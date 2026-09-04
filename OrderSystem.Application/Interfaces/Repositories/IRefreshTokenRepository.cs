using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string tokenHash);
        Task<RefreshToken?> GetActiveTokenByUserAndDeviceAsync(int userId, string deviceInfo);
        Task AddAsync(RefreshToken token);
        Task RevokeAllForUserAsync(int userId);
        Task RevokeByUserAndDeviceAsync(int userId, string deviceInfo);
        Task<int> DeleteExpiredAndRevokedAsync();
    }
}
