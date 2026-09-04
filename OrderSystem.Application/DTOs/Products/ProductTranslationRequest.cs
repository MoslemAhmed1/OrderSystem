using System.ComponentModel.DataAnnotations;

namespace OrderSystem.Application.DTOs.Products;

public record ProductTranslationRequest
{
    [Required]
    [StringLength(10)]
    public required string Culture { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }
}