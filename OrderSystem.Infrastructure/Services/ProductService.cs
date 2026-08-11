using OrderSystem.Domain;
using OrderSystem.Application.Mappings;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        private readonly ITranslationService _translation;

        public ProductService(
            IProductRepository productRepository, 
            IUnitOfWork uow, 
            ICacheService cache, 
            ITranslationService translation)
        {
            _productRepository = productRepository;
            _uow = uow;
            _cache = cache;
            _translation = translation;
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            var cacheKey = $"product_{id}";
            var cachedProduct = await _cache.GetAsync<ProductResponse>(cacheKey);
            if (cachedProduct is not null)
                return cachedProduct;

            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", id));

            var response = product.ToDto();
            await _cache.SetAsync(cacheKey, response);

            return response;
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var cacheKey = "products_all";
            var cachedProducts = await _cache.GetAsync<List<ProductResponse>>(cacheKey);
            if (cachedProducts is not null)
                return cachedProducts;

            var products = await _productRepository.GetAllAsync();
            var response = products.Select(product => product.ToDto()).ToList();

            await _cache.SetAsync(cacheKey, response);

            return response;
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var product = request.ToEntity();

            await _productRepository.AddAsync(product);
            await _uow.CommitAsync();

            var response = product.ToDto();
            await _cache.RemoveAsync("products_all");

            return response;
        }

        public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", id));

            product.UpdateFrom(request);

            await _uow.CommitAsync();

            var response = product.ToDto();
            await _cache.RemoveAsync($"product_{id}");
            await _cache.RemoveAsync("products_all");

            return response;
        }

        public async Task<DeleteResult> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return DeleteResult.NotFound;

            if (await _productRepository.IsUsedInOrdersAsync(id))
                return DeleteResult.HasExistingOrders;

            _productRepository.Delete(product);
            await _uow.CommitAsync();

            await _cache.RemoveAsync($"product_{id}");
            await _cache.RemoveAsync("products_all");
            return DeleteResult.Success;
        }
    }
}
