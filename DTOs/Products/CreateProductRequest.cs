using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs.Products
{
    public class CreateProductRequest
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        [Precision(18, 2)]
        public decimal Price { get; set; }
    }
}