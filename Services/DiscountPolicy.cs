using System.Collections.Generic;
using OrderSystem.Models;

namespace OrderSystem.Services
{
    public class DiscountPolicy : IDiscountPolicy
    {
        private static readonly Dictionary<CustomerType, decimal> _discounts = new()
        {
            { CustomerType.Regular, 1.0m },
            { CustomerType.Employee, 0.5m },
            { CustomerType.VIP, 0.8m },
            { CustomerType.WholeSale, 0.85m },
        };

        public decimal GetDiscount(CustomerType customerType)
        {
            return _discounts.TryGetValue(customerType, out var discount) ? discount : 1.0m;
        }
    }
}