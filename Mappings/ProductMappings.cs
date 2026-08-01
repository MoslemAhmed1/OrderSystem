using OrderSystem.DTOs.Products;
using OrderSystem.Models;
using OrderSystem.ViewModels;

namespace OrderSystem.Mappings
{
    public static class ProductMappings
    {
        // Entity -> Dto
        public static ProductResponse ToDto(this Product product) 
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };
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

        // Dto -> ViewModel
        public static ProductViewModel ToViewModel(this ProductResponse dto) 
        {
            return new ProductViewModel
            {
                Name = dto.Name,
                FormattedPrice = $"${dto.Price:F2}",
                IsLowStock = dto.StockQuantity < 5,
                StockStatus = dto.StockQuantity switch
                {
                    0 => "Out of Stock",
                    < 5 => "Low Stock",
                    _ => "In Stock"
                }
            };
        }
    }
}
