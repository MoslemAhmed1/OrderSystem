using Moq;
using OrderSystem.Application.DTOs.Orders;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Tests.OrderServiceTests;

public class CancelOrderAsyncTests : OrderServiceTestBase
{
    private static Order BuildOrder(int userId, OrderStatus status, List<OrderItem>? items = null)
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
            Id = 1,
            Customer = customer,
            CustomerId = customer.Id,
            Status = status,
            Items = items ?? new List<OrderItem>()
        };
    }

    [Fact]
    public async Task CancelOrderAsync_Succeeds_WhenOrderIsNew()
    {
        // Arrange
        var order = BuildOrder(userId: 5, status: OrderStatus.New);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        ProductRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>());
        UowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await Sut.CancelOrderAsync(id: 1, userId: 5, isAdmin: false);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task CancelOrderAsync_Succeeds_WhenOrderIsPaid()
    {
        // Arrange
        var order = BuildOrder(userId: 5, status: OrderStatus.Paid);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        ProductRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>());
        UowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await Sut.CancelOrderAsync(id: 1, userId: 5, isAdmin: false);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task CancelOrderAsync_RestocksItems_WhenCancelled()
    {
        // Arrange
        var product = new Product { Id = 10, Name = "Gadget", Price = 50m, StockQuantity = 7 };
        var items   = new List<OrderItem> { new() { ProductId = 10, Qty = 3 } };
        var order   = BuildOrder(userId: 5, status: OrderStatus.New, items: items);

        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        ProductRepoMock
            .Setup(r => r.GetByIdsAsync(It.Is<List<int>>(ids => ids.Contains(10))))
            .ReturnsAsync(new List<Product> { product });
        UowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await Sut.CancelOrderAsync(id: 1, userId: 5, isAdmin: false);

        // Assert
        Assert.Equal(10, product.StockQuantity);
    }

    [Fact]
    public async Task CancelOrderAsync_ThrowsKeyNotFoundException_WhenOrderNotFound()
    {
        // Arrange
        OrderRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.CancelOrderAsync(id: 99, userId: 1, isAdmin: false));
    }

    [Fact]
    public async Task CancelOrderAsync_ThrowsUnauthorizedAccessException_WhenNotOwner()
    {
        // Arrange
        var order = BuildOrder(userId: 99, status: OrderStatus.New);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => Sut.CancelOrderAsync(id: 1, userId: 5, isAdmin: false));
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task CancelOrderAsync_ThrowsInvalidOperationException_WhenStatusCannotBeCancelled(OrderStatus status)
    {
        // Arrange
        var order = BuildOrder(userId: 5, status: status);
        OrderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.CancelOrderAsync(id: 1, userId: 5, isAdmin: true));
    }
}
