using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus OrderStatus { get; set; }
    }
}