using OrderSystem.DTOs.Orders;
using OrderSystem.Models;

namespace OrderSystem.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> GetByIdAsync(int id);
        Task<List<OrderResponse>> GetAllAsync();
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse> UpdateStatusAsync(int id, OrderStatus newStatus);
        Task<OrderResponse> UpdateItemsAsync(int id, List<CreateOrderItemRequest> newItems);
        Task CancelOrderAsync(int id);
        Task DeleteAsync(int id);
    }
}