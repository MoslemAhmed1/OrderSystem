using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using OrderSystem.Common;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Orders;
using OrderSystem.Domain.Enums;

using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ITranslationService _translationService;

        public OrdersController(IOrderService orderService, ITranslationService translationService)
        {
            _orderService = orderService;
            _translationService = translationService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private bool IsAdmin() => User.IsInRole(nameof(UserRole.Admin));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var order = await _orderService.GetByIdAsync(id, GetUserId(), IsAdmin());
            return Ok(ApiResponse<OrderViewModel>.Success(order.ToViewModel(_translationService)));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync(GetUserId(), IsAdmin());
            var result = orders.Select(o => o.ToViewModel(_translationService)).ToList();
            return Ok(ApiResponse<List<OrderViewModel>>.Success(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderViewModel request)
        {
            var order = await _orderService.CreateOrderAsync(request.ToDto(), GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, ApiResponse<OrderViewModel>.Success(order.ToViewModel(_translationService), _translationService.Translate("OrderCreated"), StatusCodes.Status201Created));
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusViewModel request)
        {
            var order = await _orderService.UpdateStatusAsync(id, request.OrderStatus, GetUserId(), IsAdmin());
            return Ok(ApiResponse<OrderViewModel>.Success(order.ToViewModel(_translationService), _translationService.Translate("OrderStatusUpdated")));
        }

        [HttpPatch("{id}/items")]
        public async Task<IActionResult> UpdateItems(int id, UpdateOrderItemsViewModel request)
        {
            var requestItems = request.Items.Select(i => i.ToDto()).ToList();
            var order = await _orderService.UpdateItemsAsync(id, requestItems, GetUserId(), IsAdmin());
            return Ok(ApiResponse<OrderViewModel>.Success(order.ToViewModel(_translationService), _translationService.Translate("OrderItemsUpdated")));
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            await _orderService.CancelOrderAsync(id, GetUserId(), IsAdmin());
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Delete(int id)
        {
            await _orderService.DeleteAsync(id);
            return NoContent();
        }
    }
}