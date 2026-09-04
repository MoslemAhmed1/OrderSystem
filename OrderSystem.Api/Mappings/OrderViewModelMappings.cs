using OrderSystem.Application.DTOs.Orders;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.ViewModels.Orders;

namespace OrderSystem.Mappings
{
    public static class OrderViewModelMappings
    {
        public static CreateOrderItemRequest ToDto(this CreateOrderItemViewModel vm)
        {
            return new CreateOrderItemRequest
            {
                ProductId = vm.ProductId,
                Qty = vm.Qty
            };
        }

        public static CreateOrderRequest ToDto(this CreateOrderViewModel vm)
        {
            return new CreateOrderRequest
            {
                Items = vm.Items.Select(i => i.ToDto()).ToList()
            };
        }

        public static OrderItemViewModel ToViewModel(this OrderItemResponse dto)
        {
            return new OrderItemViewModel
            {
                Id = dto.Id,
                ProductName = dto.ProductName,
                Qty = dto.Qty,
                UnitPrice = dto.UnitPrice
            };
        }

        public static OrderViewModel ToViewModel(this OrderResponse dto, ITranslationService translationService)
        {
            return new OrderViewModel
            {
                Id = dto.Id,
                CustomerName = dto.CustomerName,
                CustomerType = translationService.Translate($"CustomerType_{dto.CustomerType}"),
                Status = translationService.Translate($"OrderStatus_{dto.Status}"),
                Total = dto.Total,
                CreatedAt = dto.CreatedAt,
                Items = dto.Items.Select(i => i.ToViewModel()).ToList()
            };
        }
    }
}
