using OrderSystem.Api.ViewModels.Products;
using OrderSystem.Application.DTOs.Products;
using OrderSystem.Application.Features.Products.Commands;
using OrderSystem.Application.Features.Products.Commands.SetProductTranslation;
using OrderSystem.Application.Features.Products.Commands.UpdateProduct;
using OrderSystem.ViewModels.Products;

namespace OrderSystem.Mappings
{
    public static class ProductViewModelMappings
    {
        // Response -> ViewModel
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

        // ViewModel -> Command
        public static CreateProductCommand ToCommand(this CreateProductViewModel vm)
        {
            return new CreateProductCommand(
                vm.Name,
                vm.Price,
                vm.StockQuantity,
                vm.Translations
                    .Select(t => new ProductTranslationRequest { Culture = t.Culture, Name = t.Name })
                    .ToList());
        }

        public static UpdateProductCommand ToCommand(this UpdateProductViewModel vm, int id)
        {
            return new UpdateProductCommand(id, vm.Name, vm.Price, vm.StockQuantity);
        }

        public static SetProductTranslationCommand ToCommand(this ProductTranslationViewModel vm, int productId)
        {
            return new SetProductTranslationCommand(productId, vm.Culture, vm.Name);
        }
    }
}
