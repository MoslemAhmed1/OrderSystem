using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using OrderSystem.DTOs.Customers;
using OrderSystem.Models;
using OrderSystem.Repositories;

namespace OrderSystem.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _uow;

        public CustomerService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<CustomerResponse?> GetByIdAsync(int id)
        {
            var customer = await _uow.Customers.GetByIdAsync(id);
            return customer is null ? null : MapToResponse(customer);
        }

        public async Task<List<CustomerResponse>> GetAllAsync()
        {
            var customers = await _uow.Customers.GetAllAsync();
            return customers.Select(MapToResponse).ToList();
        }

        public async Task<CustomerResponse> CreateAsync(CreateCustomerRequest request)
        {
            var customer = new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                CustomerType = request.CustomerType
            };

            await _uow.Customers.AddAsync(customer);
            await _uow.CommitAsync();

            return MapToResponse(customer);
        }

        public async Task<CustomerResponse?> UpdateAsync(int id, UpdateCustomerRequest request)
        {
            var customer = await _uow.Customers.GetByIdAsync(id);
            if (customer is null)
                return null;

            customer.FirstName = request.FirstName;
            customer.LastName = request.LastName;
            customer.CustomerType = request.CustomerType;

            await _uow.Customers.UpdateAsync(customer);
            await _uow.CommitAsync();

            return MapToResponse(customer);
        }

        public async Task<DeleteResult> DeleteAsync(int id)
        {

            var deleted = await _uow.Customers.DeleteAsync(id);
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

        private static CustomerResponse MapToResponse(Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                CustomerType = customer.CustomerType.ToString()
            };
        }
    }
}