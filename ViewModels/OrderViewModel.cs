namespace OrderSystem.ViewModels
{
    public class OrderViewModel
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FormattedTotal { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }
}
