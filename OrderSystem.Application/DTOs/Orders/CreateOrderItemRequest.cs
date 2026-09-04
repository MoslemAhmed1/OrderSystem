using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public record CreateOrderItemRequest
    {
        public int ProductId { get; init; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Qty { get; init; }
    }
}
