using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderSystem.DTOs.Customers;
using OrderSystem.Mappings;
using OrderSystem.Models;
using OrderSystem.Repositories;

namespace OrderSystem.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _uow;
        //private readonly IMapper _mapper;
        public CustomerService(ICustomerRepository customerRepository, IUnitOfWork uow /*, IMapper mapper*/)
        {
            _customerRepository = customerRepository;
            _uow = uow;
            //_mapper = mapper;
        }

        public async Task<CustomerResponse?> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            
            return customer is null ? null : customer.ToDto();
            //return customer is null ? null : _mapper.Map<CustomerResponse>(customer);
        }

        public async Task<List<CustomerResponse>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAllAsync();

            return customers.Select(customer => customer.ToDto()).ToList();
            //return _mapper.Map<List<CustomerResponse>>(customers);
        }

        public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
        {
            var customer = request.ToEntity();
            //var customer = _mapper.Map<Customer>(request);

            await _customerRepository.AddAsync(customer);
            await _uow.CommitAsync();

            return customer.ToDto();
            //return _mapper.Map<CustomerResponse>(customer);
        }

        public async Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer is null)
                return null;

            customer.UpdateFrom(request);
            //_mapper.Map(request, customer);

            _customerRepository.Update(customer);
            await _uow.CommitAsync();

            return customer.ToDto();
            //return _mapper.Map<CustomerResponse>(customer);
        }

        public async Task<DeleteResult> DeleteAsync(int id)
        {

            var deleted = await _customerRepository.DeleteAsync(id);
            if (!deleted)
                return DeleteResult.NotFound;

            try
            {
                await _uow.CommitAsync();
                return DeleteResult.Success;
            }
            catch (DbUpdateException)
            {
                return DeleteResult.HasExistingOrders;
            }
        }
    }
}