using Microsoft.EntityFrameworkCore;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings.Products;
using OrderSystem.Domain;

namespace OrderSystem.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;

        public ProductService(IProductRepository productRepository, IUnitOfWork uow)
        {
            _productRepository = productRepository;
            _uow = uow;
        }

        public async Task<ProductResponse> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if(product is null)
                throw new KeyNotFoundException($"Product {id} not found.");

            return product.ToDto();
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();

            return products.Select(product => product.ToDto()).ToList();
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var product = request.ToEntity();

            await _productRepository.AddAsync(product);
            await _uow.CommitAsync();

            return product.ToDto();
        }

        public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                throw new KeyNotFoundException($"Product {id} not found.");

            product.UpdateFrom(request);

            _productRepository.Update(product);
            await _uow.CommitAsync();

            return product.ToDto();
        }

        public async Task<DeleteResult> DeleteAsync(int id)
        {
            var deleted = await _productRepository.DeleteAsync(id);
            if (!deleted)
                return DeleteResult.NotFound;

            try
            {
                await _uow.CommitAsync();
                return DeleteResult.Success;
            }
            catch (DbUpdateException)
            {
                return DeleteResult.HasExistingOrders;
            }
        }
    }
}
