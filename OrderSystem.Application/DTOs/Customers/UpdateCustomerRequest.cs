using OrderSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Customers
{
    public record UpdateCustomerRequest
    {
        [Required]
        [StringLength(50)] 
        public required string FirstName { get; init; }
        
        [Required]
        [StringLength(50)] 
        public required string LastName { get; init; }
        
        public CustomerType CustomerType { get; init; }
    }
}
