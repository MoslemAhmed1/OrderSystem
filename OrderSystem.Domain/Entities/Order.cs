using OrderSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public OrderStatus Status { get; set; }

        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}