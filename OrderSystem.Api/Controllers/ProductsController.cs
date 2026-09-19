using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Api.ViewModels.Products;
using OrderSystem.Application.Features.Products.Commands;
using OrderSystem.Application.Features.Products.Queries;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Common;
using OrderSystem.Domain;
using OrderSystem.Domain.Enums;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Products;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ISender _sender;
        //private readonly IPublisher _publisher;
        //private readonly IMediator _mediator;
        
        private readonly ITranslationService _translationService;

        public ProductsController(ISender sender, ITranslationService translationService)
        {
            _sender = sender;
            _translationService = translationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _sender.Send(new GetProductByIdQuery(id));
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _sender.Send(new GetAllProductsQuery());
            var result = products.Select(p => p.ToViewModel()).ToList();
            return Ok(ApiResponse<List<ProductViewModel>>.Success(result));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Create(CreateProductViewModel request)
        {
            var product = await _sender.Send(request.ToCommand());
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, ApiResponse<ProductViewModel>.Success(product.ToViewModel(), _translationService.Translate("ProductCreated"), StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Update(int id, UpdateProductViewModel request)
        {
            var product = await _sender.Send(request.ToCommand(id));
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel(), _translationService.Translate("ProductUpdated")));
        }

        [HttpPut("{id}/translations")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> SetTranslation(int id, ProductTranslationViewModel viewModel)
        {
            var product = await _sender.Send(viewModel.ToCommand(id));
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel(), _translationService.Translate("TranslationUpdated")));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _sender.Send(new DeleteProductCommand(id));
            return result switch
            {
                DeleteResult.NotFound => NotFound(ApiResponse.Fail(_translationService.Translate("ProductNotFound", id), 404)),
                DeleteResult.HasExistingOrders => Conflict(ApiResponse.Fail(_translationService.Translate("ProductHasOrders"), 409)),
                DeleteResult.Success => NoContent(),
                _ => StatusCode(500, ApiResponse.Fail(_translationService.Translate("UnexpectedError"), 500))
            };
        }
    }
}