namespace OrderSystem.ViewModels.Orders
{
    public record OrderItemViewModel
    {
        public int Id { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Qty { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal LineTotal => UnitPrice * Qty;
    }
}
