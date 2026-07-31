using Microsoft.AspNetCore.Mvc;
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
            return order is null ? NotFound() : Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateOrderRequest request)
        {
            try
            {
                var order = await _orderService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, UpdateOrderStatusRequest request)
        {
            try
            {
                var order = await _orderService.UpdateStatusAsync(id, request.OrderStatus);
                return order is null ? NotFound() : Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("{id}/items")]
        public async Task<IActionResult> UpdateItems(int id, List<CreateOrderItemRequest> items)
        {
            try
            {
                var order = await _orderService.UpdateItemsAsync(id, items);
                return order is null ? NotFound() : Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                await _orderService.CancelOrderAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _orderService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}