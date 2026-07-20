using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required][StringLength(100)] public required string Name { get; set; }
        [Required][Precision(18, 2)][Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number")] public decimal Price { get; set; }
    }
}