using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Domain.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Qty { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be a positive number")]
        public decimal UnitPrice { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}