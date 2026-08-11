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
        void Delete(Product product);
        Task<bool> IsUsedInOrdersAsync(int id);
    }
}