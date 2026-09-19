using MediatR;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Features.Products.Commands.SetProductTranslation
{
    public record SetProductTranslationCommand(
        int ProductId,
        string Culture,
        string Name) : IRequest<ProductResponse>;

    public class SetProductTranslationCommandHandler : IRequestHandler<SetProductTranslationCommand, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICacheVersioningService _cacheVersioning;
        private readonly ITranslationService _translation;

        public SetProductTranslationCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork uow,
            ICacheVersioningService cacheVersioning,
            ITranslationService translation)
        {
            _productRepository = productRepository;
            _uow = uow;
            _cacheVersioning = cacheVersioning;
            _translation = translation;
        }

        public async Task<ProductResponse> Handle(SetProductTranslationCommand request, CancellationToken cancellationToken)
        {
            var culture = request.Culture.ToLowerInvariant();
            var product = await _productRepository.GetByIdWithTranslationsAsync(request.ProductId, culture);
            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", request.ProductId));

            var existing = await _productRepository.GetTranslationAsync(request.ProductId, culture);
            if (existing is not null)
            {
                existing.Name = request.Name;
            }
            else
            {
                var translationEntity = new ProductTranslation
                {
                    ProductId = request.ProductId,
                    Culture = culture,
                    Name = request.Name
                };
                product.Translations.Add(translationEntity);
                await _productRepository.AddTranslationAsync(translationEntity);
            }

            await _uow.SaveChangesAsync();
            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);

            return product.ToDto();
        }
    }
}
