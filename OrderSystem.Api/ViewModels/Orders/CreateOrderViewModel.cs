using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Orders
{
    public class CreateOrderViewModel
    {
        [Required]
        public int CustomerId { get; set; }

        public List<CreateOrderItemViewModel> Items { get; set; } = new List<CreateOrderItemViewModel>();
    }
}
