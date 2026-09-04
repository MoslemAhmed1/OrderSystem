using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.DTOs.Orders
{
    public record UpdateOrderStatusRequest
    {
        public OrderStatus OrderStatus { get; init; }
    }
}
