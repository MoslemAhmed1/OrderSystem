using OrderSystem.Application.DTOs.Products;
using OrderSystem.ViewModels.Products;

namespace OrderSystem.Mappings
{
    public static class ProductViewModelMappings
    {
        public static CreateProductRequest ToDto(this CreateProductViewModel vm)
        {
            return new CreateProductRequest
            {
                Name = vm.Name,
                Price = vm.Price,
                StockQuantity = vm.StockQuantity
            };
        }

        public static UpdateProductRequest ToDto(this UpdateProductViewModel vm)
        {
            return new UpdateProductRequest
            {
                Name = vm.Name,
                Price = vm.Price,
                StockQuantity = vm.StockQuantity
            };
        }

        public static ProductViewModel ToViewModel(this ProductResponse dto)
        {
            return new ProductViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity
            };
        }
    }
}
