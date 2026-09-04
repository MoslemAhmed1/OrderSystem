using OrderSystem.Api.ViewModels.Products;
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
                StockQuantity = vm.StockQuantity,
                Translations = vm.Translations
                    .Select(t => new ProductTranslationRequest { Culture = t.Culture, Name = t.Name })
                    .ToList()
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

        public static ProductTranslationRequest ToDto(this ProductTranslationViewModel vm)
        {
            return new ProductTranslationRequest 
            { 
                Culture = vm.Culture, 
                Name = vm.Name 
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
