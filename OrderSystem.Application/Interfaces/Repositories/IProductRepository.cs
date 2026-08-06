using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(int id);
        Task<List<Product>> GetByIdsAsync(List<int> ids);
        Task<List<Product>> GetAllAsync();
        Task AddAsync(Product product);
        void Update(Product product);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsUsedInOrdersAsync(int id);
    }
}