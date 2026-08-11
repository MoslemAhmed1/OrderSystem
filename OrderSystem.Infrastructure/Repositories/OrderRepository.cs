using Microsoft.EntityFrameworkCore;

using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Data;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _orderContext.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _orderContext.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllByCustomerIdAsync(int customerId)
        {
            return await _orderContext.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Items).ThenInclude(i => i.Product)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            await _orderContext.Orders.AddAsync(order);
        }

        public void Update(Order order)
        {
            _orderContext.Orders.Update(order);
        }

        public void Delete(Order order)
        {
            _orderContext.Orders.Remove(order);
        }

    }
}
