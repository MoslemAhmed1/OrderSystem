namespace OrderSystem.Application.DTOs.Orders
{
    public record OrderItemResponse
    {
        public int Id { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Qty { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal Total => UnitPrice * Qty;
    }
}
