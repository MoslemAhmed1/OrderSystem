using System.ComponentModel.DataAnnotations;
using OrderSystem.Domain.Enums;

namespace OrderSystem.ViewModels.Customers
{
    public class UpdateCustomerViewModel
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required]
        public CustomerType CustomerType { get; set; }
    }
}