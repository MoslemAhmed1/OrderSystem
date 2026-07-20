using OrderSystem.Models;
using System.ComponentModel.DataAnnotations;

public class CreateCustomerRequest
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