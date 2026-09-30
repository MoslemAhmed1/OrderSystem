using Microsoft.EntityFrameworkCore;
using OrderSystem.Application.Features.Products.Queries;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Domain.Entities;
using OrderSystem.Infrastructure.Context;

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
        public async Task<Product?> GetByIdWithTranslationsAsync(int id, string culture)
        {
            return await _orderContext.Products.Include(p => p.Translations.Where(t => t.Culture == culture)).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetAllWithTranslationsAsync(string culture)
        {
            return await _orderContext.Products.AsNoTracking().Include(p => p.Translations.Where(t => t.Culture == culture)).ToListAsync();
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

        public async Task<(List<Product> Products, int TotalCount)> GetPagedAsync(string culture, ProductQueryParameters queryParams)
        {
            var query = _orderContext.Products
                .AsNoTracking()
                .Include(p => p.Translations.Where(t => t.Culture == culture))
                .AsQueryable();

            // Filter
            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var term = queryParams.Search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Translations.Any(t => t.Culture == culture && t.Name.ToLower().Contains(term)));
            }

            if (queryParams.MinPrice.HasValue)
                query = query.Where(p => p.Price >= queryParams.MinPrice.Value);

            if (queryParams.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= queryParams.MaxPrice.Value);

            // Total count
            var totalCount = await query.CountAsync();

            // Sort
            query = queryParams.SortBy?.ToLowerInvariant() switch
            {
                "price" => queryParams.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "stockquantity" => queryParams.SortDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity)
                _ => query
            };

            // Pagination
            var products = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return (products, totalCount);
        }
    }
}
