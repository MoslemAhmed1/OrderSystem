using Moq;
using OrderSystem.Application.DTOs.Orders;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

namespace OrderSystem.Tests.OrderServiceTests;
public class CreateOrderAsyncTests : OrderServiceTestBase
{
    private static Customer BuildCustomer(CustomerType type = CustomerType.Regular) => new()
    {
        Id = 1,
        UserId = 1,
        FirstName = "Moslem",
        LastName = "Ahmed",
        CustomerType = type
    };

    private static Product BuildProduct(int id = 1, decimal price = 100m, int stock = 10) => new()
    {
        Id = id,
        Name = "Asus TUF",
        Price = price,
        StockQuantity = stock,
        Translations = new List<ProductTranslation>()
    };

    private static CreateOrderRequest BuildRequest(int productId = 1, int qty = 2) => new()
    {
        Items = new List<CreateOrderItemRequest>
        {
            new() { ProductId = productId, Qty = qty }
        }
    };

    [Fact]
    public async Task CreateOrderAsync_ThrowsArgumentException_WhenItemsListIsEmpty()
    {
        // Arrange
        var request = new CreateOrderRequest { Items = new List<CreateOrderItemRequest>() };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => Sut.CreateOrderAsync(request, userId: 1));
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsKeyNotFoundException_WhenCustomerNotFound()
    {
        // Arrange
        CustomerRepoMock
            .Setup(r => r.GetByUserIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.CreateOrderAsync(BuildRequest(), userId: 1));
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsKeyNotFoundException_WhenProductNotFound()
    {
        // Arrange
        CustomerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(BuildCustomer());
        ProductRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product>());

        var request = BuildRequest(99, 1);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut.CreateOrderAsync(request, userId: 1));
    }

    [Fact]
    public async Task CreateOrderAsync_ThrowsInvalidOperationException_WhenInsufficientStock()
    {
        // Arrange
        var product = BuildProduct(stock: 3);
        CustomerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(BuildCustomer());
        ProductRepoMock
            .Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>()))
            .ReturnsAsync(new List<Product> { product });

        var request = BuildRequest(productId: 1, qty: 10);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => Sut.CreateOrderAsync(request, userId: 1));
    }

    [Theory]
    [InlineData(CustomerType.Regular, 1.0, 200.0)]
    [InlineData(CustomerType.VIP, 0.8, 160.0)]
    [InlineData(CustomerType.Employee, 0.50, 100.0)]
    [InlineData(CustomerType.WholeSale, 0.85, 170.0)]
    public async Task CreateOrderAsync_AppliesCorrectDiscount_PerCustomerType(CustomerType type, decimal discount, decimal expectedTotal)
    {
        // Arrange
        var customer = BuildCustomer(type);
        var product  = BuildProduct(id: 1, price: 100m, stock: 10);

        CustomerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(customer);
        ProductRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Product> { product });
        DiscountPolicyMock.Setup(d => d.GetDiscount(type)).Returns(discount);
        UowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        OrderRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                foreach (var item in order.Items)
                    item.Product = product;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await Sut.CreateOrderAsync(BuildRequest(productId: 1, qty: 2), userId: 1);

        // Assert
        Assert.Equal(expectedTotal, result.Total);
    }

    [Fact]
    public async Task CreateOrderAsync_CorrectlyDeductsStock()
    {
        // Arrange
        var customer = BuildCustomer();
        var product  = BuildProduct(stock: 10);

        CustomerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(customer);
        ProductRepoMock.Setup(r => r.GetByIdsAsync(It.IsAny<List<int>>())).ReturnsAsync(new List<Product> { product });
        DiscountPolicyMock.Setup(d => d.GetDiscount(It.IsAny<CustomerType>())).Returns(1.0m);
        UowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        
        OrderRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                foreach (var item in order.Items)
                    item.Product = product;
            })
            .Returns(Task.CompletedTask);

        // Act
        await Sut.CreateOrderAsync(BuildRequest(), userId: 1);

        // Assert
        UowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        Assert.Equal(8, product.StockQuantity);
    }
}
