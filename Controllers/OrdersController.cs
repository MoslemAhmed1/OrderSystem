using Microsoft.AspNetCore.Mvc;
using OrderSystem.Common;
using OrderSystem.DTOs.Orders;
using OrderSystem.Services;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id);
            return Ok(ApiResponse<OrderResponse>.Success(order));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(ApiResponse<List<OrderResponse>>.Success(orders));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            var order = await _orderService.CreateOrderAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, ApiResponse<OrderResponse>.Success(order, "Order created successfully.", StatusCodes.Status201Created));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
        {
            var order = await _orderService.UpdateStatusAsync(id, request.OrderStatus);
            return Ok(ApiResponse<OrderResponse>.Success(order, "Order status updated."));
        }

        [HttpPatch("{id}/items")]
        public async Task<IActionResult> UpdateItems(int id, List<CreateOrderItemRequest> items)
        {
            var order = await _orderService.UpdateItemsAsync(id, items);
            return Ok(ApiResponse<OrderResponse>.Success(order, "Order items updated."));
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            await _orderService.CancelOrderAsync(id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return NoContent();
        }
    }
}