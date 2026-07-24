using Microsoft.EntityFrameworkCore;
using OrderSystem.Data;
using OrderSystem.Models;

namespace OrderSystem.Repositories
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
        public async Task<List<Product>> GetAllAsync()
        {
            return await _orderContext.Products.ToListAsync();
        }
        public async Task AddAsync(Product product)
        {
            await _orderContext.Products.AddAsync(product);
        }
        public void Update(Product product)
        {
            _orderContext.Products.Update(product);
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _orderContext.Products.FindAsync(id);
            if (product != null)
            {
                _orderContext.Products.Remove(product);
                return true;
            }
            return false;
        }
    }
}
