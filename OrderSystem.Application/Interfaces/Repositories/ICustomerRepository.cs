using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<List<Customer>> GetAllAsync();
        Task AddAsync(Customer customer);
        void Update(Customer customer);
        Task<bool> DeleteAsync(int id);
        Task<bool> IsUsedInOrdersAsync(int id);
    }
}