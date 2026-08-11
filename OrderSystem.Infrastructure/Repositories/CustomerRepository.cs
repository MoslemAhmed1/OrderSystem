using Microsoft.EntityFrameworkCore;

using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Data;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly OrderContext _orderContext;

        public CustomerRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }
        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _orderContext.Customers.FindAsync(id);
        }

        public async Task<Customer?> GetByUserIdAsync(int userId)
        {
            return await _orderContext.Customers
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _orderContext.Customers.AsNoTracking().ToListAsync();
        }
        public async Task AddAsync(Customer customer)
        {
            await _orderContext.Customers.AddAsync(customer);
        }
        public void Update(Customer customer)
        {
            _orderContext.Customers.Update(customer);
        }

        public void Delete(Customer customer)
        {
            _orderContext.Customers.Remove(customer);
        }

        public async Task<bool> IsUsedInOrdersAsync(int id)
        {
            return await _orderContext.Orders.AnyAsync(o => o.CustomerId == id);
        }
    }
}
