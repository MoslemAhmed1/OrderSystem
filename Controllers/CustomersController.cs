using Microsoft.AspNetCore.Mvc;
using OrderSystem.DTOs.Customers;
using OrderSystem.Services;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            return customer is null ? NotFound() : Ok(customer);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerRequest request)
        {
            var customer = await _customerService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCustomerRequest request)
        {
            var customer = await _customerService.UpdateAsync(id, request);
            return customer is null ? NotFound() : Ok(customer);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService.DeleteAsync(id);
            return result switch
            {
                DeleteResult.NotFound => NotFound(),
                DeleteResult.HasExistingOrders => Conflict("Cannot delete a customer that has existing orders."),
                DeleteResult.Success => NoContent(),
                _ => StatusCode(500)
            };
        }
    }
}