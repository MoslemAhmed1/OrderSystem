using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<List<Order>> GetAllAsync();
        Task<List<Order>> GetAllByCustomerIdAsync(int customerId);
        Task AddAsync(Order order);
        void Update(Order order);
        void Delete(Order order);
    }
}