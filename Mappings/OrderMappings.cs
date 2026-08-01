using OrderSystem.DTOs.Orders;
using OrderSystem.Models;
using OrderSystem.ViewModels;

namespace OrderSystem.Mappings
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

        // Dto -> ViewModel
        public static OrderViewModel ToViewModel(this OrderResponse dto)
        {
            return new OrderViewModel
            {
                OrderId = $"#{dto.Id:D4}",
                CustomerName = dto.CustomerName,
                Status = dto.Status,
                FormattedTotal = $"${dto.Total:N2}",
                OrderDate = dto.CreatedAt.ToString("MMM dd, yyyy"),
                ItemCount = dto.Items.Count
            };
        }
    }
}
