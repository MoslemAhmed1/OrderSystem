using System.ComponentModel.DataAnnotations;
using OrderSystem.Api.ViewModels.Products;

namespace OrderSystem.ViewModels.Products
{
    public record CreateProductViewModel
    {
        [Required]
        [StringLength(100)]
        public required string Name { get; init; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number")]
        public decimal Price { get; init; }

        [Range(0, int.MaxValue, ErrorMessage = "StockQuantity must be a positive number")]
        public int StockQuantity { get; init; }

        public List<ProductTranslationViewModel> Translations { get; init; } = new();
    }
}
