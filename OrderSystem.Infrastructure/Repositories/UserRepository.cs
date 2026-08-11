using Microsoft.EntityFrameworkCore;

using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Data;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly OrderContext _orderContext;

        public UserRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _orderContext.Users.FindAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _orderContext.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task CreateAsync(User user)
        {
            await _orderContext.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _orderContext.Users.Update(user);
        }

        public async Task<(bool UsernameExists, bool EmailExists)> CheckUserExistsAsync(string username, string email)
        {
            var matches = await _orderContext.Users
                .Where(u => u.Username.ToLower() == username.ToLower() || u.Email.ToLower() == email.ToLower())
                .Select(u => new { u.Username, u.Email })
                .ToListAsync();
            
            return (
                matches.Any(u => u.Username.ToLower() == username.ToLower()),
                matches.Any(u => u.Email.ToLower() == email.ToLower())
            );
        }
    }
}
