using OrderSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Orders
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus OrderStatus { get; set; }
    }
}