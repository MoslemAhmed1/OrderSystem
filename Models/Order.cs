using Microsoft.EntityFrameworkCore;
using OrderSystem.Models;

public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    public OrderStatus Status { get; set; }

    [Precision(18, 2)]
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}