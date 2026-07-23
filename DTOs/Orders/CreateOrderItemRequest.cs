using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs.Orders
{
    public class CreateOrderItemRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Qty { get; set; }
    }
}
