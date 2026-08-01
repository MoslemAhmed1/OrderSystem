using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrderSystem.DTOs.Products;
using OrderSystem.Mappings;
using OrderSystem.Models;
using OrderSystem.Repositories;

namespace OrderSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        //private readonly IMapper _mapper;

        public ProductService(IProductRepository productRepository, IUnitOfWork uow /*, IMapper mapper*/)
        {
            _productRepository = productRepository;
            _uow = uow;
            //_mapper = mapper;
        }

        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            return product is null ? null : product.ToDto();
            //return product is null ? null : _mapper.Map<ProductResponse>(product);
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var products = await _productRepository.GetAllAsync();

            return products.Select(product => product.ToDto()).ToList();
            //return _mapper.Map<List<ProductResponse>>(products);
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var product = request.ToEntity();
            //var product = _mapper.Map<Product>(request);

            await _productRepository.AddAsync(product);
            await _uow.CommitAsync();

            return product.ToDto();
            //return _mapper.Map<ProductResponse>(product);
        }

        public async Task<ProductResponse?> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product is null)
                return null;

            product.UpdateFrom(request);
            //_mapper.Map(request, product);

            _productRepository.Update(product);
            await _uow.CommitAsync();

            return product.ToDto();
            //return _mapper.Map<ProductResponse>(product);
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
