using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs.Products
{
    public class UpdateProductRequest
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        [Precision(18, 2)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "StockQuantity must be a positive number")]
        public int StockQuantity { get; set; }
    }
}
