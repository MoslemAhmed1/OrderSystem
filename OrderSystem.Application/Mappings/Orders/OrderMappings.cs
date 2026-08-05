using OrderSystem.Application.DTOs.Orders;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Mappings.Orders
{
    public static class OrderMappings
    {
        // Entity -> Dto
        public static OrderResponse ToDto(this Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                CustomerType = order.Customer.CustomerType.ToString(),
                Status = order.Status.ToString(),
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(item => new OrderItemResponse
                {
                    Id = item.Id,
                    ProductName = item.Product.Name,
                    Qty = item.Qty,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
        }
    }
}
