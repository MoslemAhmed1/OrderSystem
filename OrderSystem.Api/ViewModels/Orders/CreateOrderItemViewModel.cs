using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Orders
{
    public class CreateOrderItemViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        public int Qty { get; set; }
    }
}
