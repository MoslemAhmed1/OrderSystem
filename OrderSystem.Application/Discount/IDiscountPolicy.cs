using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Discount
{
    public interface IDiscountPolicy
    {
        decimal GetDiscount(CustomerType customerType);
    }
}