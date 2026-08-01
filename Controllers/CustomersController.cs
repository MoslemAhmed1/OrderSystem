using Microsoft.AspNetCore.Mvc;
using OrderSystem.Common;
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

            if (customer is null)
                return NotFound(ApiResponse<CustomerResponse>.Fail("Customer not found.", StatusCodes.Status404NotFound));

            return Ok(ApiResponse<CustomerResponse>.Success(customer));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(ApiResponse<List<CustomerResponse>>.Success(customers));
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerRequest request)
        {
            var customer = await _customerService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ApiResponse<CustomerResponse>.Success(customer, "Customer created successfully.", StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateCustomerRequest request)
        {
            var customer = await _customerService.UpdateAsync(id, request);

            if (customer is null)
                return NotFound(ApiResponse<CustomerResponse>.Fail("Customer not found.", 404));

            return Ok(ApiResponse<CustomerResponse>.Success(customer, "Customer updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService.DeleteAsync(id);
            return result switch
            {
                DeleteResult.NotFound => NotFound(ApiResponse.Fail("Customer not found.", 404)),
                DeleteResult.HasExistingOrders => Conflict(ApiResponse.Fail("Cannot delete a customer that has existing orders.", 409)),
                DeleteResult.Success => NoContent(),
                _ => StatusCode(500, ApiResponse.Fail("Unexpected error.", 500))
            };
        }
    }
}