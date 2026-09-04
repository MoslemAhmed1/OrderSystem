using OrderSystem.Application.DTOs.Products;
using OrderSystem.Domain.Entities;
using System.Globalization;

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
            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            var translation = product.Translations?.FirstOrDefault(t => t.Culture == culture);
            return translation?.Name ?? product.Name; // fallback
        }

        // Dto -> Entity
        public static Product ToEntity(this CreateProductRequest request) 
        {
            return new Product
            {
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };
        }

        public static void UpdateFrom(this Product entity, UpdateProductRequest request)
        {
            entity.Name = request.Name;
            entity.Price = request.Price;
            entity.StockQuantity = request.StockQuantity;
        }
    }
}
