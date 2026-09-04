namespace OrderSystem.Application.DTOs.Orders
{
    public record OrderResponse
    {
        public int Id { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal Total { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<OrderItemResponse> Items { get; init; } = new();
    }
}
