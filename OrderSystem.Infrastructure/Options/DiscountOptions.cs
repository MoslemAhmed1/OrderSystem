namespace OrderSystem.Infrastructure.Options
{
    public class DiscountOptions
    {
        public decimal Regular { get; set; }
        public decimal Employee { get; set; }
        public decimal VIP { get; set; }
        public decimal WholeSale { get; set; }
    }
}
