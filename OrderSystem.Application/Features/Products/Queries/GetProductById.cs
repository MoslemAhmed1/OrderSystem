using MediatR;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using System.Globalization;

namespace OrderSystem.Application.Features.Products.Queries
{
    public record GetProductByIdQuery(int Id) : IRequest<ProductResponse>;

    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ICacheService _cache;
        private readonly ICacheVersioningService _cacheVersioning;
        private readonly ITranslationService _translation;

        public GetProductByIdHandler(
            IProductRepository productRepository,
            ICacheService cache,
            ICacheVersioningService cacheVersioning,
            ITranslationService translation)
        {
            _productRepository = productRepository;
            _cache = cache;
            _cacheVersioning = cacheVersioning;
            _translation = translation;
        }

        public async Task<ProductResponse> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var version = await _cacheVersioning.GetVersionAsync(CacheKeys.ProductsVersion);
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var cacheKey = $"products:v{version}:{request.Id}:{culture}";

            var cachedProduct = await _cache.GetAsync<ProductResponse>(cacheKey);
            if (cachedProduct is not null)
                return cachedProduct;

            var product = await _productRepository.GetByIdWithTranslationsAsync(request.Id, culture);
            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", request.Id));

            var response = product.ToDto();
            await _cache.SetAsync(cacheKey, response);

            return response;
        }
    }
}
