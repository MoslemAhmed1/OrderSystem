using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Orders
{
    public record CreateOrderItemViewModel
    {
        [Required]
        public int ProductId { get; init; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Qty { get; init; }
    }
}
