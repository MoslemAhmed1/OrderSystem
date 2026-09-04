using OrderSystem.Domain.Entities;
using OrderSystem.Application.DTOs.Customers;

namespace OrderSystem.Application.Mappings
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
        public static void UpdateFrom(this Customer entity, UpdateCustomerRequest request)
        {
            entity.FirstName = request.FirstName;
            entity.LastName = request.LastName;
            entity.CustomerType = request.CustomerType;
        }
    }
}