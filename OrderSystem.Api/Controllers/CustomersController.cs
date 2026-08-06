using Microsoft.AspNetCore.Mvc;
using OrderSystem.Common;
using OrderSystem.Domain;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Customers;
using OrderSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ITranslationService _translationService;

        public CustomersController(ICustomerService customerService, ITranslationService translationService)
        {
            _customerService = customerService;
            _translationService = translationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            return Ok(ApiResponse<CustomerViewModel>.Success(customer.ToViewModel()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            var result = customers.Select(c => c.ToViewModel()).ToList();
            return Ok(ApiResponse<List<CustomerViewModel>>.Success(result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateCustomerViewModel request)
        {
            var customer = await _customerService.CreateAsync(request.ToDto());
            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ApiResponse<CustomerViewModel>.Success(customer.ToViewModel(), _translationService.Translate("CustomerCreated"), StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, UpdateCustomerViewModel request)
        {
            var customer = await _customerService.UpdateAsync(id, request.ToDto());
            return Ok(ApiResponse<CustomerViewModel>.Success(customer.ToViewModel(), _translationService.Translate("CustomerUpdated")));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _customerService.DeleteAsync(id);
            return result switch
            {
                DeleteResult.NotFound => NotFound(ApiResponse.Fail(_translationService.Translate("CustomerNotFound", id), StatusCodes.Status404NotFound)),
                DeleteResult.HasExistingOrders => Conflict(ApiResponse.Fail(_translationService.Translate("CustomerHasOrders"), StatusCodes.Status409Conflict)),
                DeleteResult.Success => NoContent(),
                _ => StatusCode(500, ApiResponse.Fail(_translationService.Translate("UnexpectedError"), StatusCodes.Status500InternalServerError))
            };
        }
    }
}