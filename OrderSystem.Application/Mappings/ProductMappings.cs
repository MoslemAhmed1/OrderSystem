using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Features.Products.Commands;
using OrderSystem.Application.Features.Products.Commands.UpdateProduct;
using OrderSystem.Domain.Entities;

namespace OrderSystem.Application.Mappings
{
    public static class ProductMappings
    {
        // Entity -> Dto
        public static ProductResponse ToDto(this Product product) 
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.ResolveName(),
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };
        }

        public static string ResolveName(this Product product)
        {
            var translation = product.Translations?.FirstOrDefault();
            return translation?.Name ?? product.Name; // fallback
        }

        // Command -> Entity
        public static Product ToEntity(this CreateProductCommand request) 
        {
            return new Product
            {
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };
        }

        public static void UpdateFrom(this Product entity, UpdateProductCommand request)
        {
            entity.Name = request.Name;
            entity.Price = request.Price;
            entity.StockQuantity = request.StockQuantity;
        }
    }
}
