using OrderSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        [Required] 
        public OrderStatus OrderStatus { get; set; }
    }
}