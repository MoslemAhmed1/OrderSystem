using OrderSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.ViewModels.Orders
{
    public record UpdateOrderStatusViewModel
    {
        [Required]
        public OrderStatus OrderStatus { get; init; }
    }
}
