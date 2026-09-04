using System.ComponentModel.DataAnnotations;
using OrderSystem.Domain.Enums;

namespace OrderSystem.ViewModels.Customers
{
    public record UpdateCustomerViewModel
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; init; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; init; }

        [Required]
        public CustomerType CustomerType { get; init; }
    }
}
