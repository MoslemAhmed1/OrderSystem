using OrderSystem.Domain.Enums;
using OrderSystem.Application.DTOs.Orders;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> GetByIdAsync(int id, int userId, bool isAdmin);
        Task<List<OrderResponse>> GetAllAsync(int userId, bool isAdmin);
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, int userId);
        Task<OrderResponse> UpdateStatusAsync(int id, OrderStatus newStatus, int userId, bool isAdmin);
        Task<OrderResponse> UpdateItemsAsync(int id, List<CreateOrderItemRequest> newItems, int userId, bool isAdmin);
        Task CancelOrderAsync(int id, int userId, bool isAdmin);
        Task DeleteAsync(int id);
    }
}