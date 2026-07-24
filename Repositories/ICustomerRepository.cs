using OrderSystem.Models;

namespace OrderSystem.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
        Task<List<Customer>> GetAllAsync();
        Task AddAsync(Customer customer);
        void Update(Customer customer);
        Task<bool> DeleteAsync(int id);
    }
}