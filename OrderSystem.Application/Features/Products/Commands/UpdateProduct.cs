using MediatR;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;

namespace OrderSystem.Application.Features.Products.Commands.UpdateProduct
{
    public record UpdateProductCommand(
        int Id,
        string Name,
        decimal Price,
        int StockQuantity) : IRequest<ProductResponse>;

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICacheVersioningService _cacheVersioning;
        private readonly ITranslationService _translation;

        public UpdateProductCommandHandler(
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

        public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
                throw new KeyNotFoundException(_translation.Translate("ProductNotFound", request.Id));

            product.UpdateFrom(request);

            await _uow.SaveChangesAsync();

            var response = product.ToDto();
            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);

            return response;
        }
    }
}
