using OrderSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Customers
{
    public class CreateCustomerRequest
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        public CustomerType CustomerType { get; set; }
    }
}