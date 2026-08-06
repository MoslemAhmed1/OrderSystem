using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }

        // Minimum 1 item is enforced in the service layer (MinLength does not work on List<T>)
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}