using OrderSystem.Application.DTOs.Customers;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Mappings.Customers
{
    public static class CustomerMappings
    {
        // Entity -> Dto
        public static CustomerResponse ToDto(this Customer customer)
        {
            return new CustomerResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                CustomerType = customer.CustomerType.ToString()
            };
        }

        // Dto -> Entity
        public static Customer ToEntity(this CreateCustomerRequest request)
        {
            return new Customer
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                CustomerType = request.CustomerType
            };
        }

        public static void UpdateFrom(this Customer entity, UpdateCustomerRequest request)
        {
            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.CustomerType = request.CustomerType;
        }
    }
}