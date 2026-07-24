using OrderSystem.Models;

namespace OrderSystem.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<List<Order>> GetAllAsync();
        Task AddAsync(Order order);
        void Update(Order order);
        Task<bool> DeleteAsync(int id);
    }
}