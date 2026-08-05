namespace OrderSystem.ViewModels.Orders
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderItemViewModel> Items { get; set; } = new();
    }
}
