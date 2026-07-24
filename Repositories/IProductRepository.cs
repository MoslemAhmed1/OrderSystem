using OrderSystem.Models;
using OrderSystem.Services;

namespace OrderSystem.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<List<Product>> GetAllAsync();
        Task AddAsync(Product product);
        void Update(Product product);
        Task<bool> DeleteAsync(int id);
    }
}