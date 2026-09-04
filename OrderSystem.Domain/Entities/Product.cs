namespace OrderSystem.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        public ICollection<ProductTranslation> Translations { get; set; } = new List<ProductTranslation>();
    }
}