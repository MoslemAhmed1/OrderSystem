using OrderSystem.Domain;
using OrderSystem.Application.Mappings;
using OrderSystem.Application.DTOs.Customers;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _uow;
        private readonly ITranslationService _translation;

        public CustomerService(ICustomerRepository customerRepository, IUnitOfWork uow, ITranslationService translation)
        {
            _customerRepository = customerRepository;
            _uow = uow;
            _translation = translation;
        }

        public async Task<CustomerResponse> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            
            if (customer is null)
                throw new KeyNotFoundException(_translation.Translate("CustomerNotFound", id));

            return customer.ToDto();
        }

        public async Task<List<CustomerResponse>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAllAsync();

            return customers.Select(customer => customer.ToDto()).ToList();
        }

        public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
        {
            var customer = request.ToEntity();

            await _customerRepository.AddAsync(customer);
            await _uow.CommitAsync();

            return customer.ToDto();
        }

        public async Task<CustomerResponse> UpdateAsync(int id, UpdateCustomerRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer is null)
                throw new KeyNotFoundException(_translation.Translate("CustomerNotFound", id));

            customer.UpdateFrom(request);

            await _uow.CommitAsync();

            return customer.ToDto();
        }

        public async Task<CustomerResponse> UpdateSelfAsync(int userId, UpdateCustomerProfileRequest request)
        {
            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer is null)
                throw new KeyNotFoundException(_translation.Translate("CustomerNotFound", userId));

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;

            await _uow.CommitAsync();

            return customer.ToDto();
        }

        public async Task<DeleteResult> DeleteAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer is null)
                return DeleteResult.NotFound;

            if (await _customerRepository.IsUsedInOrdersAsync(id))
                return DeleteResult.HasExistingOrders;

            _customerRepository.Delete(customer);
            await _uow.CommitAsync();

            return DeleteResult.Success;
        }
    }
}