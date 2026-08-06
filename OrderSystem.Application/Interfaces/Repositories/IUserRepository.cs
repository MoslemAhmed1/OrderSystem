using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task CreateAsync(User user);
        void Update(User user);
        Task<(bool UsernameExists, bool EmailExists)> CheckUserExistsAsync(string username, string email);
    }
}
