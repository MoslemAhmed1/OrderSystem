using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Orders
{
    public class CreateOrderViewModel
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Order must contain at least one item.")]
        public List<CreateOrderItemViewModel> Items { get; set; } = new();
    }
}
