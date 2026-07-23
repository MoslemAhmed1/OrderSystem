namespace OrderSystem.DTOs.Orders
{
    public class OrderItemResponse
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => UnitPrice * Qty;
    }
}