using OrderSystem.Application.DTOs.Customers;
using OrderSystem.Domain;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<CustomerResponse> GetByIdAsync(int id);
        Task<List<CustomerResponse>> GetAllAsync();
        Task<CustomerResponse> CreateAsync(CreateCustomerRequest request);
        Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request);
        Task<DeleteResult> DeleteAsync(int id);
    }
}
