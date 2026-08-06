using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Qty { get; set; }
    }
}
