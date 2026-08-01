using Microsoft.AspNetCore.Mvc;
using OrderSystem.DTOs.Products;
using OrderSystem.Mappings;
using OrderSystem.Services;

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
            return product is null ? NotFound() : Ok(product);
        }

        [HttpGet("{id}/view")]
        public async Task<IActionResult> GetByIdAsViewModel(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product is null)
                return NotFound();

            var viewModel = product.ToViewModel();   
            // var viewModel = _mapper.Map<ProductViewModel>(product);

            return Ok(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productService.GetAllAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            var product = await _productService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductRequest request)
        {
            var product = await _productService.UpdateAsync(id, request);
            return product is null ? NotFound() : Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.DeleteAsync(id);
            return result switch
            {
                DeleteResult.NotFound => NotFound(),
                DeleteResult.HasExistingOrders => Conflict("Cannot delete a product that has existing orders."),
                DeleteResult.Success => NoContent(),
                _ => StatusCode(500)
            };
        }
    }
}