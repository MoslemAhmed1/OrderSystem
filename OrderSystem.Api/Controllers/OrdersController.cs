using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using OrderSystem.Common;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Orders;

using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsAdmin() => User.IsInRole("Admin");

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id, GetUserId(), IsAdmin());
            return Ok(ApiResponse<OrderViewModel>.Success(order.ToViewModel()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync(GetUserId(), IsAdmin());
            var result = orders.Select(o => o.ToViewModel()).ToList();
            return Ok(ApiResponse<List<OrderViewModel>>.Success(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderViewModel request)
        {
            var order = await _orderService.CreateOrderAsync(request.ToDto(), GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, ApiResponse<OrderViewModel>.Success(order.ToViewModel(), "Order created successfully.", StatusCodes.Status201Created));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusViewModel request)
        {
            var order = await _orderService.UpdateStatusAsync(id, request.OrderStatus, GetUserId(), IsAdmin());
            return Ok(ApiResponse<OrderViewModel>.Success(order.ToViewModel(), "Order status updated."));
        }

        [HttpPatch("{id}/items")]
        public async Task<IActionResult> UpdateItems(int id, UpdateOrderItemsViewModel request)
        {
            var requestItems = request.Items.Select(i => i.ToDto()).ToList();
            var order = await _orderService.UpdateItemsAsync(id, requestItems, GetUserId(), IsAdmin());
            return Ok(ApiResponse<OrderViewModel>.Success(order.ToViewModel(), "Order items updated."));
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            await _orderService.CancelOrderAsync(id, GetUserId(), IsAdmin());
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return NoContent();
        }
    }
}