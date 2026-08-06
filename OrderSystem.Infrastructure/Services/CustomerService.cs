using OrderSystem.Application.DTOs.Customers;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using OrderSystem.Domain;

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

            _customerRepository.Update(customer);
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

            await _customerRepository.DeleteAsync(id);
            await _uow.CommitAsync();

            return DeleteResult.Success;
        }
    }
}