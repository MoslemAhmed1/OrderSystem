using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public class CreateOrderRequest
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}