using Microsoft.IdentityModel.Tokens;
using OrderSystem.DTOs.Orders;
using OrderSystem.Models;
using OrderSystem.Repositories;

namespace OrderSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;
        private readonly IDiscountPolicy _discountPolicy;
        public OrderService(IUnitOfWork uow, IDiscountPolicy discountPolicy)
        {
            _uow = uow;
            _discountPolicy = discountPolicy;
        }
        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            var order = await _uow.Orders.GetByIdAsync(id);
            return (order is null ? null : MapToResponse(order));
        }
        public async Task<List<OrderResponse>> GetAllAsync()
        {
            var orders = await _uow.Orders.GetAllAsync();
            return orders.Select(MapToResponse).ToList();
        }
        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var customer = await _uow.Customers.GetByIdAsync(request.CustomerId);
            if (customer is null)
                throw new InvalidOperationException($"Customer {request.CustomerId} not found.");

            var order = new Order
            {
                CustomerId = customer.Id,
                //Customer = customer, TODO: this or refetch from databse for customer to be populated in OrderResponse DTO?
                Status = OrderStatus.New,
                //Items = new List<OrderItem>()
            };

            decimal total = 0m;
            foreach (var itemRequest in request.Items)
            {
                var product = await _uow.Products.GetByIdAsync(itemRequest.ProductId);
                if (product is null)
                    throw new InvalidOperationException($"Product {itemRequest.ProductId} not found.");

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    //Product = product, TODO: this or refetch from databse for product to be populated in OrderResponse DTO?
                    Qty = itemRequest.Qty,
                    UnitPrice = product.Price
                };

                order.Items.Add(orderItem);
                total += orderItem.UnitPrice * orderItem.Qty;
            }

            var discount = _discountPolicy.GetDiscount(customer.CustomerType);
            order.Total = total * discount;

            await _uow.Orders.AddAsync(order);
            await _uow.CommitAsync();

            var savedOrder = await _uow.Orders.GetByIdAsync(order.Id); // TODO: this or populate customer & product above ?
            return MapToResponse(savedOrder!);
        }

        public async Task<OrderResponse?> UpdateStatusAsync(int id, OrderStatus newStatus)
        {
            var order = await _uow.Orders.GetByIdAsync(id);
            if (order is null) 
                return null;

            order.Status = newStatus;
            await _uow.CommitAsync();

            return MapToResponse(order);
        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                CustomerName = $"{order.Customer.FirstName} {order.Customer.LastName}",
                CustomerType = order.Customer.CustomerType.ToString(),
                Status = order.Status.ToString(),
                Total = order.Total,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductName = i.Product.Name,
                    Qty = i.Qty,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
