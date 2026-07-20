using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required] public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Required][Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")] public int Qty { get; set; }
        [Required][Precision(18, 2)][Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be a positive number")] public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}