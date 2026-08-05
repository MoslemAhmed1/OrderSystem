using Microsoft.EntityFrameworkCore;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Data;

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

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _orderContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task CreateAsync(User user)
        {
            await _orderContext.Users.AddAsync(user);
        }

        public void Update(User user)
        {
            _orderContext.Users.Update(user);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _orderContext.Users
                .AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _orderContext.Users
                .AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }
    }
}
