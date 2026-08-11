using Microsoft.EntityFrameworkCore;

using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Data;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly OrderContext _orderContext;

        public ProductRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }
        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _orderContext.Products.FindAsync(id);
        }
        public async Task<List<Product>> GetByIdsAsync(List<int> ids)
        {
            return await _orderContext.Products.Where(p => ids.Contains(p.Id)).ToListAsync();
        }
        public async Task<List<Product>> GetAllAsync()
        {
            return await _orderContext.Products.AsNoTracking().ToListAsync();
        }
        public async Task AddAsync(Product product)
        {
            await _orderContext.Products.AddAsync(product);
        }
        public void Update(Product product)
        {
            _orderContext.Products.Update(product);
        }
        public void Delete(Product product)
        {
            _orderContext.Products.Remove(product);
        }

        public async Task<bool> IsUsedInOrdersAsync(int id)
        {
            return await _orderContext.OrderItems.AnyAsync(oi => oi.ProductId == id);
        }
    }
}
