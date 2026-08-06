using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "StockQuantity must be a positive number")]
        public int StockQuantity { get; set; } = 0;
    }
}