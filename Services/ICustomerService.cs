using OrderSystem.DTOs.Customers;

namespace OrderSystem.Services
{
    public interface ICustomerService
    {
        Task<CustomerResponse?> GetByIdAsync(int id);
        Task<List<CustomerResponse>> GetAllAsync();
        Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);
        Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request);
        Task<DeleteResult> DeleteAsync(int id);
    }
}
