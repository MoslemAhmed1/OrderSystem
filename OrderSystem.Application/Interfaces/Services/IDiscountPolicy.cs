using OrderSystem.Domain.Enums;

namespace OrderSystem.Application.Interfaces.Services
{
    public interface IDiscountPolicy
    {
        decimal GetDiscount(CustomerType customerType);
    }
}