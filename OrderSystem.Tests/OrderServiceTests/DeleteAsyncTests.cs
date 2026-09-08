using Moq;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Tests.OrderServiceTests;

public class DeleteAsyncTests : OrderServiceTestBase
{
    private static Order BuildOrder(int id, OrderStatus status) => new()
    {
        Id = id,
        Customer = new Customer
        {
            Id = 1, UserId = 1,
            FirstName = "Moslem", LastName = "Ahmed",
            CustomerType = CustomerType.Regular
        },
        CustomerId = 1,
        Status = status,
        IsDeleted = false,
        Items = new List<OrderItem>()
    };

    [Fact]
    public async Task DeleteAsync_SetsIsDeletedToTrue_WhenOrderIsCancelled()
    {
        // Arrange
        var order = BuildOrder(id: 1, status: OrderStatus.Cancelled);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        UowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await Sut.DeleteAsync(id: 1);

        // Assert
        Assert.True(order.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        OrderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.DeleteAsync(id: 99));
        UowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Theory]
    [InlineData(OrderStatus.New)]
    [InlineData(OrderStatus.Paid)]
    [InlineData(OrderStatus.Shipped)]
    public async Task DeleteAsync_ThrowsInvalidOperationException_WhenOrderIsNotCancelled(OrderStatus status)
    {
        // Arrange
        var order = BuildOrder(id: 1, status: status);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.DeleteAsync(id: 1));
        UowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
}
