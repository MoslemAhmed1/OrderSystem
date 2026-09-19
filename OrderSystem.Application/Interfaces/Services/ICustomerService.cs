using OrderSystem.Domain;
using OrderSystem.Application.DTOs.Customers;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface ICustomerService
    {
        Task<CustomerResponse> GetByIdAsync(int id);
        Task<List<CustomerResponse>> GetAllAsync();
        Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request); // TODO (ignore): 2 update functions, one for type, another for names
        Task<CustomerResponse> UpdateSelfAsync(int userId, UpdateCustomerProfileRequest request);
        Task<DeleteResult> DeleteAsync(int id);
    }
}
