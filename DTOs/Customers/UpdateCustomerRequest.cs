using OrderSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs.Customers
{
    public class UpdateCustomerRequest
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
