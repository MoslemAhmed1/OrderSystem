namespace OrderSystem.ViewModels
{
    public class ProductViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string FormattedPrice { get; set; } = string.Empty;
        public bool IsLowStock { get; set; } 
        public string StockStatus { get; set; } = string.Empty;
    }
}
