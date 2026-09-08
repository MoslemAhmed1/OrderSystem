namespace OrderSystem.Domain.Entities;

public class ProductTranslation
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public required string Culture { get; set; }

    public required string Name { get; set; }
}