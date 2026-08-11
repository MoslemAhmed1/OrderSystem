using OrderSystem.Domain.Enums;

namespace OrderSystem.Infrastructure.Discount
{
    public interface IDiscountPolicy
    {
        decimal GetDiscount(CustomerType customerType);
    }
}