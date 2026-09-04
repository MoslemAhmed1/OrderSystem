namespace OrderSystem.ViewModels.Orders
{
    public record OrderViewModel
    {
        public int Id { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerType { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal Total { get; init; }
        public DateTime CreatedAt { get; init; }
        public List<OrderItemViewModel> Items { get; init; } = new();
    }
}
