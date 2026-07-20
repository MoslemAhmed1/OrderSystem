using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        
        [Required] public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required][Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")] public int Qty { get; set; }
        [Required][Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be a positive number")] public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
    }
}