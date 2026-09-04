using Microsoft.EntityFrameworkCore;

using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Context;
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

        // Translations
        public async Task<Product?> GetByIdWithTranslationsAsync(int id)
        {
            return await _orderContext.Products.Include(p => p.Translations).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetAllWithTranslationsAsync()
        {
            return await _orderContext.Products.AsNoTracking().Include(p => p.Translations).ToListAsync();
        }

        public async Task AddTranslationAsync(ProductTranslation translation)
        {
            await _orderContext.ProductTranslations.AddAsync(translation);
        }

        public async Task<ProductTranslation?> GetTranslationAsync(int productId, string culture)
        {
            //return await _orderContext.ProductTranslations.FirstOrDefaultAsync(t => t.ProductId == productId && t.Culture == culture);
            return await _orderContext.ProductTranslations.FindAsync(productId, culture);
        }
    }
}
