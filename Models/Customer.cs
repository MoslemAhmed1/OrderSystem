using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models
{
    public enum CustomerType
    {
        Regular,
        Employee,
        VIP,
        WholeSale
    }

    public class Customer
    {
        public int Id { get; set; }

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