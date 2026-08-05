namespace OrderSystem.ViewModels.Orders
{
    public class OrderItemViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Qty;
    }
}
