using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Models;

namespace OrderSystem.Repositories
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
        public async Task<List<Customer>> GetAllAsync()
        {
            return await _orderContext.Customers.ToListAsync();
        }
        public async Task AddAsync(Customer customer)
        {
            await _orderContext.Customers.AddAsync(customer);
        }
        public async Task UpdateAsync(Customer customer)
        {
            _orderContext.Customers.Update(customer);
            await Task.CompletedTask;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _orderContext.Customers.FindAsync(id);
            if (customer != null)
            {
                _orderContext.Customers.Remove(customer);
                return true;
            }
            return false;
        }
    }
}
