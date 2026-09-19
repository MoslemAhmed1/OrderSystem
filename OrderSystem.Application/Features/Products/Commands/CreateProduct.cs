using MediatR;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Features.Products.Commands
{
    public record CreateProductCommand(
        string Name,
        decimal Price,
        int StockQuantity,
        List<ProductTranslationRequest> Translations) : IRequest<ProductResponse>;

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICacheVersioningService _cacheVersioning;

        public CreateProductCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork uow,
            ICacheVersioningService cacheVersioning)
        {
            _productRepository = productRepository;
            _uow = uow;
            _cacheVersioning = cacheVersioning;
        }

        public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = request.ToEntity();

            await _productRepository.AddAsync(product);

            foreach (var t in request.Translations)
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
    }
}
