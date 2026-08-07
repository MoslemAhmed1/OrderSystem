using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string tokenHash);
        Task<RefreshToken?> GetActiveTokenByUserAndDeviceAsync(int userId, string deviceInfo);
        Task CreateAsync(RefreshToken token);
        Task<int> RevokeAllForUserAsync(int userId);
        Task<int> DeleteExpiredAndRevokedAsync();
    }
}
