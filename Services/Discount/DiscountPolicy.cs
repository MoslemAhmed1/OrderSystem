using Microsoft.Extensions.Options;
using OrderSystem.Models;

namespace OrderSystem.Services.Discount
{
    public class DiscountPolicy : IDiscountPolicy
    {
        private readonly IOptionsMonitor<DiscountOptions> _discountOptions;
        public DiscountPolicy(IOptionsMonitor<DiscountOptions> discountOptions)
        {
            _discountOptions = discountOptions;
        }

        public decimal GetDiscount(CustomerType customerType)
        {
            var discountOptions = _discountOptions.CurrentValue;
            return customerType switch
            {
                CustomerType.Regular => discountOptions.Regular,
                CustomerType.Employee => discountOptions.Employee,
                CustomerType.VIP => discountOptions.VIP,
                CustomerType.WholeSale => discountOptions.WholeSale,
                _ => 1.0m
            };
        }
    }
}