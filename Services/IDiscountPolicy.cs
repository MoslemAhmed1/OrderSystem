using OrderSystem.Models;

namespace OrderSystem.Services
{
    public interface IDiscountPolicy
    {
        decimal GetDiscount(CustomerType customerType);
    }
}