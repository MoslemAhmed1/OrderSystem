using Moq;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Tests.OrderServiceTests;

public class GetAllAsyncTests : OrderServiceTestBase
{
    // GetAllAsync has 3 test cases:
    // 1. Admin requests orders -> Returns all orders
    // 2. Customer requests orders -> Returns only customer orders
    // 3. Customer does not exist -> Throws KeyNotFoundException

    private static Customer BuildCustomer(int id, int userId) => new()
    {
        Id = id,
        UserId = userId,
        FirstName = "Moslem",
        LastName = "Ahmed",
        CustomerType = CustomerType.Regular
    };

    private static Order BuildOrder(int id, Customer customer) => new()
    {
        Id = id,
        Customer = customer,
        CustomerId = customer.Id,
        Status = OrderStatus.New,
        Items = new List<OrderItem>()
    };

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders_WhenCalledByAdmin()
    {
        // Arrange
        var customer = BuildCustomer(id: 1, userId: 10);
        var orders = new List<Order>
        {
            BuildOrder(1, customer),
            BuildOrder(2, customer)
        };
        OrderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        // Act
        var result = await Sut.GetAllAsync(userId: 1, isAdmin: true);

        // Assert
        Assert.Equal(2, result.Count);

        // Verify GetAllAsync is called (not GetAllByCustomerIdAsync)
        OrderRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
        OrderRepoMock.Verify(r => r.GetAllByCustomerIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyCustomerOrders_WhenCalledByCustomer()
    {
        // Arrange
        var customers = new List<Customer> 
        { 
            BuildCustomer(id: 1, userId: 5),
            BuildCustomer(id: 2, userId: 6)
        };
        var orders = new List<Order> { BuildOrder(1, customers[0]) };

        CustomerRepoMock.Setup(r => r.GetByUserIdAsync(5)).ReturnsAsync(customers[0]);
        OrderRepoMock.Setup(r => r.GetAllByCustomerIdAsync(1)).ReturnsAsync(orders);

        // Act
        var result = await Sut.GetAllAsync(userId: 5, isAdmin: false);

        // Assert
        Assert.Single(result);
        OrderRepoMock.Verify(r => r.GetAllByCustomerIdAsync(1), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ThrowsKeyNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        CustomerRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<int>())).ReturnsAsync((Customer?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.GetAllAsync(userId: 5, isAdmin: false));
    }
}
