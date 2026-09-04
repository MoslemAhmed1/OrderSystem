using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Api.ViewModels.Products;

public record ProductTranslationViewModel
{
    [Required]
    [StringLength(10)]
    public required string Culture { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
}