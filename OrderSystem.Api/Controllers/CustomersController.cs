using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using OrderSystem.Common;
using OrderSystem.Domain;
using OrderSystem.Domain.Enums;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Customers;

using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ITranslationService _translationService;

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public CustomersController(ICustomerService customerService, ITranslationService translationService)
        {
            _customerService = customerService;
            _translationService = translationService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            return Ok(ApiResponse<CustomerViewModel>.Success(customer.ToViewModel(_translationService)));
        }

        [HttpGet]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _customerService.GetAllAsync();
            var result = customers.Select(c => c.ToViewModel(_translationService)).ToList();
            return Ok(ApiResponse<List<CustomerViewModel>>.Success(result));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Update(int id, UpdateCustomerViewModel request)
        {
            var customer = await _customerService.UpdateAsync(id, request.ToDto());
            return Ok(ApiResponse<CustomerViewModel>.Success(customer.ToViewModel(_translationService), _translationService.Translate("CustomerUpdated")));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateSelf(UpdateCustomerProfileViewModel request)
        {
            var customer = await _customerService.UpdateSelfAsync(GetUserId(), request.ToDto());
            return Ok(ApiResponse<CustomerViewModel>.Success(customer.ToViewModel(_translationService), _translationService.Translate("CustomerUpdated")));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
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