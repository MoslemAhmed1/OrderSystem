using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OrderSystem.Common;
using OrderSystem.Domain;
using OrderSystem.Mappings;
using OrderSystem.ViewModels.Products;
using OrderSystem.Application.Interfaces.Services;

namespace OrderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            var result = products.Select(p => p.ToViewModel()).ToList();
            return Ok(ApiResponse<List<ProductViewModel>>.Success(result));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreateProductViewModel request)
        {
            var product = await _productService.CreateAsync(request.ToDto());
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, ApiResponse<ProductViewModel>.Success(product.ToViewModel(), "Product created successfully.", StatusCodes.Status201Created));
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, UpdateProductViewModel request)
        {
            var product = await _productService.UpdateAsync(id, request.ToDto());
            return Ok(ApiResponse<ProductViewModel>.Success(product.ToViewModel(), "Product updated successfully."));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            return result switch
            {
                DeleteResult.NotFound => NotFound(ApiResponse.Fail("Product not found.", 404)),
                DeleteResult.HasExistingOrders => Conflict(ApiResponse.Fail("Cannot delete a product that has existing orders.", 409)),
                DeleteResult.Success => NoContent(),
                _ => StatusCode(500, ApiResponse.Fail("Unexpected error.", 500))
            };
        }
    }
}