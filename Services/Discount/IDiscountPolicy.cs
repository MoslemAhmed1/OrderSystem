using OrderSystem.Models;

namespace OrderSystem.Services.Discount
{
    public interface IDiscountPolicy
    {
        decimal GetDiscount(CustomerType customerType);
    }
}