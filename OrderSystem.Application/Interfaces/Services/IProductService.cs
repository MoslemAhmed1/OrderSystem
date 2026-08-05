using OrderSystem.Application.DTOs.Products;
using OrderSystem.Domain;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface IProductService
    {
        Task<ProductResponse> GetByIdAsync(int id);
        Task<List<ProductResponse>> GetAllAsync();
        Task<ProductResponse> CreateAsync(CreateProductRequest request);
        Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
        Task<DeleteResult> DeleteAsync(int id);
    }
}
