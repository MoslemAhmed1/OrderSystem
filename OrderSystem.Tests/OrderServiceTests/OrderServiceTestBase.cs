using Moq;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Infrastructure.Services;

namespace OrderSystem.Tests.OrderServiceTests;

public abstract class OrderServiceTestBase
{
    public readonly Mock<IOrderRepository> OrderRepoMock = new();
    public readonly Mock<ICustomerRepository> CustomerRepoMock = new();
    public readonly Mock<IProductRepository> ProductRepoMock = new();
    public readonly Mock<IUnitOfWork> UowMock = new();
    public readonly Mock<IDiscountPolicy> DiscountPolicyMock = new();
    public readonly Mock<ITranslationService> TranslationMock = new();
    public readonly Mock<ICacheVersioningService> CacheVersioningMock = new();

    public readonly OrderService Sut;
    public OrderServiceTestBase()
    {
        TranslationMock
            .Setup(t => t.Translate(It.IsAny<string>()))
            .Returns("translated message");

        TranslationMock
            .Setup(t => t.Translate(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns("translated message");

        CacheVersioningMock
            .Setup(c => c.UpdateVersionAsync(It.IsAny<string>()))
            .ReturnsAsync(1);

        Sut = new OrderService(
            OrderRepoMock.Object,
            CustomerRepoMock.Object,
            ProductRepoMock.Object,
            UowMock.Object,
            DiscountPolicyMock.Object,
            TranslationMock.Object,
            CacheVersioningMock.Object);
    }
}
