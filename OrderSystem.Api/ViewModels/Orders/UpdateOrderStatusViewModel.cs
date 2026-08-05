using OrderSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Orders
{
    public class UpdateOrderStatusViewModel
    {
        [Required]
        public OrderStatus OrderStatus { get; set; }
    }
}
