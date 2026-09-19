using MediatR;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Domain;

namespace OrderSystem.Application.Features.Products.Commands
{
    public record DeleteProductCommand(int Id) : IRequest<DeleteResult>;

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DeleteResult>
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICacheVersioningService _cacheVersioning;

        public DeleteProductCommandHandler(
            IProductRepository productRepository,
            IUnitOfWork uow,
            ICacheVersioningService cacheVersioning)
        {
            _productRepository = productRepository;
            _uow = uow;
            _cacheVersioning = cacheVersioning;
        }

        public async Task<DeleteResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product is null)
                return DeleteResult.NotFound;

            if (await _productRepository.IsUsedInOrdersAsync(request.Id))
                return DeleteResult.HasExistingOrders;

            _productRepository.Delete(product);
            await _uow.SaveChangesAsync();

            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);
            return DeleteResult.Success;
        }
    }
}
