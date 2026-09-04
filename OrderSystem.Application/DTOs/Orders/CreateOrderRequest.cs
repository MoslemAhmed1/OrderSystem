using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public record CreateOrderRequest
    {
        public List<CreateOrderItemRequest> Items { get; init; } = new();
    }
}
