using Moq;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Tests.OrderServiceTests;

public class UpdateStatusAsyncTests : OrderServiceTestBase
{

    private static Order BuildOrder(int id, int userId, OrderStatus status)
    {
        var customer = new Customer
        {
            Id = 1,
            UserId = userId,
            FirstName = "Moslem",
            LastName = "Ahmed",
            CustomerType = CustomerType.Regular
        };

        return new Order
        {
            Id = id,
            Customer = customer,
            CustomerId = customer.Id,
            Status = status,
            Items = new List<OrderItem>()
        };
    }

    [Fact]
    public async Task UpdateStatusAsync_ThrowsKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        OrderRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.UpdateStatusAsync(1, OrderStatus.Paid, userId: 1, isAdmin: true));
    }

    [Fact]
    public async Task UpdateStatusAsync_ThrowsUnauthorizedAccessException_WhenCustomerUpdatesAnotherUsersOrder()
    {
        // Arrange
        var order = BuildOrder(id: 1, userId: 99, status: OrderStatus.New);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Sut.UpdateStatusAsync(1, OrderStatus.Paid, userId: 5, isAdmin: false));
    }

    [Theory]
    [InlineData(OrderStatus.New, OrderStatus.Paid)]
    [InlineData(OrderStatus.Paid, OrderStatus.Shipped)]
    [InlineData(OrderStatus.New, OrderStatus.Cancelled)]
    public async Task UpdateStatusAsync_Succeeds_ForValidTransitions(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Arrange
        var order = BuildOrder(id: 1, userId: 10, status: currentStatus);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        UowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await Sut.UpdateStatusAsync(1, newStatus, userId: 10, isAdmin: true);

        // Assert
        Assert.Equal(newStatus.ToString(), result.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Shipped, OrderStatus.New)]
    [InlineData(OrderStatus.Shipped, OrderStatus.Paid)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.New)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Paid)]
    [InlineData(OrderStatus.Paid, OrderStatus.New)]
    public async Task UpdateStatusAsync_ThrowsInvalidOperationException_ForInvalidTransitions(OrderStatus currentStatus, OrderStatus newStatus)
    {
        // Arrange
        var order = BuildOrder(id: 1, userId: 10, status: currentStatus);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.UpdateStatusAsync(1, newStatus, userId: 10, isAdmin: true));
    }
}
