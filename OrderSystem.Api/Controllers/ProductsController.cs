using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderSystem.Api.ViewModels.Products;
using OrderSystem.Application.DTOs.Products;
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
        private readonly IMediator _mediator;
        private readonly ITranslationService _translationService;

        public ProductsController(IMediator mediator, ITranslationService translationService)
        {
            _mediator = mediator;
            _translationService = translationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _mediator.Send(new GetProductByIdQuery(id));
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameters queryParams)
        {
            var paged = await _mediator.Send(new GetAllProductsQuery(queryParams));

            var viewModel = new PagedResponse<ProductViewModel>
            {
                Items      = paged.Items.Select(p => p.ToViewModel()).ToList(),
                TotalCount = paged.TotalCount,
                Page       = paged.Page,
                PageSize   = paged.PageSize
            };

            return Ok(ApiResponse<PagedResponse<ProductViewModel>>.Success(viewModel));
        }

        [HttpPost]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Create(CreateProductViewModel request)
        {
            var product = await _mediator.Send(request.ToCommand());
            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                ApiResponse<ProductViewModel>.Success(product.ToViewModel(), _translationService.Translate("ProductCreated"), StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Update(int id, UpdateProductViewModel request)
        {
            var product = await _mediator.Send(request.ToCommand(id));
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel(), _translationService.Translate("ProductUpdated")));
        }

        [HttpPut("{id}/translations")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> SetTranslation(int id, ProductTranslationViewModel viewModel)
        {
            var product = await _mediator.Send(viewModel.ToCommand(id));
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel(), _translationService.Translate("TranslationUpdated")));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(UserRole.Admin))]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteProductCommand(id));
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
