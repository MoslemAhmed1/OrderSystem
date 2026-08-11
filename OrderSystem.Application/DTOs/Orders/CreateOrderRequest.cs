using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public class CreateOrderRequest
    {
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}