using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Models
{
    public enum OrderStatus
    {
        New,
        Paid,
        Shipped
    }

    public class Order
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;

        public OrderStatus Status { get; set; }

        [Precision(18, 2)]
        public decimal Total { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime CreatedAt { get; set; }
        
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}