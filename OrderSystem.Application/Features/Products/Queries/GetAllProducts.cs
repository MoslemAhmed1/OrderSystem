using MediatR;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using System.Globalization;

namespace OrderSystem.Application.Features.Products.Queries
{
    public record GetAllProductsQuery() : IRequest<List<ProductResponse>>;

    public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, List<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cache;
        private readonly ICacheVersioningService _cacheVersioning;

        public GetAllProductsHandler(
            IProductRepository productRepository,
            ICacheService cache,
            ICacheVersioningService cacheVersioning)
        {
            _productRepository = productRepository;
            _cache = cache;
            _cacheVersioning = cacheVersioning;
        }

        public async Task<List<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var version = await _cacheVersioning.GetVersionAsync(CacheKeys.ProductsVersion);
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var cacheKey = $"products:v{version}:all:{culture}";

            var cachedProducts = await _cache.GetAsync<List<ProductResponse>>(cacheKey);
            if (cachedProducts is not null)
                return cachedProducts;

            var products = await _productRepository.GetAllWithTranslationsAsync(culture);
            var response = products.Select(product => product.ToDto()).ToList();

            await _cache.SetAsync(cacheKey, response);

            return response;
        }
    }
}
