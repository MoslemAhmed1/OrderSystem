using OrderSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        [Required] 
        public OrderStatus OrderStatus { get; set; }
    }
}