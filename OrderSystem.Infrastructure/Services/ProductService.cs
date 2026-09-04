using OrderSystem.Application;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using OrderSystem.Domain;
using OrderSystem.Domain.Entities;
using System.Globalization;

namespace OrderSystem.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICacheService _cache;
        private readonly ICacheVersioningService _cacheVersioning;
        private readonly ITranslationService _translation;

        public ProductService(
            IProductRepository productRepository, 
            IUnitOfWork uow, 
            ICacheService cache, 
            ICacheVersioningService cacheVersioning,
            ITranslationService translation)
        {
            _productRepository = productRepository;
            _uow = uow;
            _cache = cache;
            _cacheVersioning = cacheVersioning;
            _translation = translation;
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            var version = await _cacheVersioning.GetVersionAsync(CacheKeys.ProductsVersion);
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var cacheKey = $"products:v{version}:{id}:{culture}";

            var cachedProduct = await _cache.GetAsync<ProductResponse>(cacheKey);
            if (cachedProduct is not null)
                return cachedProduct;

            var product = await _productRepository.GetByIdWithTranslationsAsync(id);
            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", id));

            var response = product.ToDto();
            await _cache.SetAsync(cacheKey, response);

            return response;
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var version = await _cacheVersioning.GetVersionAsync(CacheKeys.ProductsVersion);
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var cacheKey = $"products:v{version}:all:{culture}";

            var cachedProducts = await _cache.GetAsync<List<ProductResponse>>(cacheKey);
            if (cachedProducts is not null)
                return cachedProducts;

            var products = await _productRepository.GetAllWithTranslationsAsync();
            var response = products.Select(product => product.ToDto()).ToList();

            await _cache.SetAsync(cacheKey, response);

            return response;
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var product = request.ToEntity();

            await _productRepository.AddAsync(product);
            
            foreach(var t in request.Translations) // TODO: stack all tasks then call WhenAll ?
            {
                var translation = new ProductTranslation
                {
                    Product = product,
                    Culture = t.Culture.ToLowerInvariant(),
                    Name = t.Name
                };
                product.Translations.Add(translation);
                await _productRepository.AddTranslationAsync(translation);
            }

            await _uow.SaveChangesAsync();

            var response = product.ToDto();
            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);

            return response;
        }

        public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", id));

            product.UpdateFrom(request);

            await _uow.SaveChangesAsync();

            var response = product.ToDto();
            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);

            return response;
        }

        public async Task<ProductResponse> SetTranslationAsync(int productId, ProductTranslationRequest request)
        {
            var product = await _productRepository.GetByIdWithTranslationsAsync(productId);
            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", productId));

            var culture = request.Culture.ToLowerInvariant();
            var existing = await _productRepository.GetTranslationAsync(productId, culture);
            if (existing is not null)
            {
                existing.Name = request.Name;
            }
            else
            {
                var translation = new ProductTranslation
                {
                    ProductId = productId,
                    Culture = culture,
                    Name = request.Name
                };
                product.Translations.Add(translation);
                await _productRepository.AddTranslationAsync(translation);
            }

            await _uow.SaveChangesAsync();
            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);

            return product.ToDto();
        }

        public async Task<DeleteResult> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return DeleteResult.NotFound;

            if (await _productRepository.IsUsedInOrdersAsync(id))
                return DeleteResult.HasExistingOrders;

            _productRepository.Delete(product);
            await _uow.SaveChangesAsync();

            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);
            return DeleteResult.Success;
        }
    }
}
