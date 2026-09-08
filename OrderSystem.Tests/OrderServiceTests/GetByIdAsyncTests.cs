using Moq;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Tests.OrderServiceTests;

public class GetByIdAsyncTests : OrderServiceTestBase
{
    // GetByIdAsync has 4 test cases:
    // 1. Admin requests any order -> Returns order
    // 2. Customer requests own order -> Returns order
    // 3. Customer requests another user's order -> Throws UnauthorizedAccessException
    // 4. Order does not exist -> Throws KeyNotFoundException

    private static Order BuildOrder(int orderId, int customerId, int userId)
    {
        var customer = new Customer
        {
            Id = customerId,
            UserId = userId,
            FirstName = "Moslem",
            LastName = "Ahmed",
            CustomerType = CustomerType.Regular
        };

        return new Order
        {
            Id = orderId,
            CustomerId = customerId,
            Customer = customer,
            Status = OrderStatus.New,
            Total = 100m,
            Items = new List<OrderItem>()
        };
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrder_WhenAdminRequestsAnyOrder()
    {
        // Arrange
        var order = BuildOrder(orderId: 1, customerId: 1, userId: 1);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act
        var result = await Sut.GetByIdAsync(id: 1, userId: 1, isAdmin: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Moslem Ahmed", result.CustomerName);
        Assert.Equal("New", result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrder_WhenCustomerRequestsOwnOrder()
    {
        // Arrange
        var order = BuildOrder(orderId: 1, customerId: 1, userId: 1);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act
        var result = await Sut.GetByIdAsync(id: 1, userId: 1, isAdmin: false);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        OrderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.GetByIdAsync(id: 99, userId: 1, isAdmin: false));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.GetByIdAsync(id: 99, userId: 1, isAdmin: true));
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsUnauthorizedAccessException_WhenCustomerAccessesAnotherUsersOrder()
    {
        // Arrange
        var order = BuildOrder(orderId: 1, customerId: 10, userId: 99); // order belongs to user 99
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Sut.GetByIdAsync(id: 1, userId: 5, isAdmin: false)); // requester userid is 5
    }
}
