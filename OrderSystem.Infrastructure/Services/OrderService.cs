using OrderSystem.Domain.Enums;
using OrderSystem.Domain.Entities;

using OrderSystem.Application;
using OrderSystem.Application.Mappings;
using OrderSystem.Application.DTOs.Orders;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly IDiscountPolicy _discountPolicy;
        private readonly ITranslationService _translation;
        private readonly ICacheVersioningService _cacheVersioning;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IUnitOfWork uow,
            IDiscountPolicy discountPolicy,
            ITranslationService translation,
            ICacheVersioningService cacheVersioning)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _uow = uow;
            _discountPolicy = discountPolicy;
            _translation = translation;
            _cacheVersioning = cacheVersioning;
        }
        
        public async Task<OrderResponse> GetByIdAsync(int id, int userId, bool isAdmin)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if (order is null)
                throw new KeyNotFoundException(_translation.Translate("OrderNotFound", id));

            if(!isAdmin)
            {
                if (order.Customer.UserId != userId)
                    throw new UnauthorizedAccessException(_translation.Translate("OrderAccessDenied"));
            }
            
            return order.ToDto();
        }
        
        public async Task<List<OrderResponse>> GetAllAsync(int userId, bool isAdmin) // TODO (ignore): should be 2 separate methods for admin and customer
        {
            List<Order> orders;

            if (isAdmin)
            {
                orders = await _orderRepository.GetAllAsync();
            }
            else
            {
                var customer = await _customerRepository.GetByUserIdAsync(userId);
                if(customer is null)
                    throw new KeyNotFoundException(_translation.Translate("CustomerNotFound"));

                orders = await _orderRepository.GetAllByCustomerIdAsync(customer.Id);
            }

            return orders.Select(order => order.ToDto()).ToList();
        }
        
        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, int userId)
        {
            if (request.Items is null || request.Items.Count == 0)
                throw new ArgumentException(_translation.Translate("OrderItemsEmpty"));

            var customer = await _customerRepository.GetByUserIdAsync(userId);
            if (customer is null)
                throw new KeyNotFoundException(_translation.Translate("CustomerNotFound", userId));

            var items = await BuildItems(request.Items);
            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer,
                Status = OrderStatus.New,
                Items = items
            };
            
            order.Total = CalculateTotal(items, customer.CustomerType);

            await _orderRepository.AddAsync(order);
            await _uow.SaveChangesAsync();

            await InvalidateProductCacheAsync();

            return order.ToDto();
        }

        public async Task<OrderResponse> UpdateStatusAsync(int id, OrderStatus newStatus, int userId, bool isAdmin)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException(_translation.Translate("OrderNotFound", id));

            if (!isAdmin && order.Customer.UserId != userId)
                throw new UnauthorizedAccessException(_translation.Translate("OrderAccessDenied"));

            if (!IsValidTransition(order.Status, newStatus))
                throw new InvalidOperationException(_translation.Translate("OrderStatusTransitionInvalid", order.Status, newStatus));

            order.Status = newStatus;
            await _uow.SaveChangesAsync();

            return order.ToDto();
        }

        public async Task<OrderResponse> UpdateItemsAsync(int id, List<CreateOrderItemRequest> newItems, int userId, bool isAdmin)
        {
            if (newItems is null || newItems.Count == 0)
                throw new ArgumentException(_translation.Translate("OrderItemsEmpty"));

            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException(_translation.Translate("OrderNotFound", id));

            if (!isAdmin && order.Customer.UserId != userId)
                throw new UnauthorizedAccessException(_translation.Translate("OrderAccessDenied"));

            if (order.Status != OrderStatus.New)
                throw new InvalidOperationException(_translation.Translate("OrderItemsCannotUpdate"));

            await RestockItems(order.Items);

            var items = await BuildItems(newItems);
            order.Items.Clear();
            foreach (var item in items)
                order.Items.Add(item);

            order.Total = CalculateTotal(items, order.Customer.CustomerType);

            await _uow.SaveChangesAsync();

            await InvalidateProductCacheAsync();

            return order.ToDto();
        }

        public async Task CancelOrderAsync(int id, int userId, bool isAdmin)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException(_translation.Translate("OrderNotFound", id));

            if (!isAdmin && order.Customer.UserId != userId)
                throw new UnauthorizedAccessException(_translation.Translate("OrderAccessDenied"));

            if (order.Status != OrderStatus.New && order.Status != OrderStatus.Paid)
                throw new InvalidOperationException(_translation.Translate("OrderCancelInvalid", order.Status));
            
            order.Status = OrderStatus.Cancelled;
            await RestockItems(order.Items);

            await _uow.SaveChangesAsync();

            await InvalidateProductCacheAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException(_translation.Translate("OrderNotFound", id));

            if (order.Status != OrderStatus.Cancelled)
                throw new InvalidOperationException(_translation.Translate("OrderDeleteInvalid"));

            order.IsDeleted = true;
            await _uow.SaveChangesAsync();
        }

        private async Task<List<OrderItem>> BuildItems(List<CreateOrderItemRequest> items)
        {
            var orderItems = new List<OrderItem>();
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productRepository.GetByIdsAsync(productIds);
            
            foreach (var item in items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is null)
                    throw new KeyNotFoundException(_translation.Translate("ProductNotFound", item.ProductId));
                
                if (product.StockQuantity < item.Qty)
                    throw new InvalidOperationException(_translation.Translate("InsufficientStock", product.Name, item.Qty, product.StockQuantity));

                product.StockQuantity -= item.Qty;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Qty = item.Qty,
                    UnitPrice = product.Price
                };
                orderItems.Add(orderItem);
            }
            
            return orderItems;
        }

        private async Task RestockItems(ICollection<OrderItem> items)
        {
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productRepository.GetByIdsAsync(productIds);

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is not null)
                    product.StockQuantity += item.Qty;
            }
        }

        private async Task InvalidateProductCacheAsync()
        {
            await _cacheVersioning.UpdateVersionAsync(CacheKeys.ProductsVersion);
        }

        private decimal CalculateTotal(List<OrderItem> items, CustomerType customerType)
        {
            var total = items.Sum(i => i.Qty * i.UnitPrice);
            var discount = _discountPolicy.GetDiscount(customerType);
            return total * discount;
        }

        private static bool IsValidTransition(OrderStatus current, OrderStatus next)
        {
            return (current, next) switch
            {
                (OrderStatus.New, OrderStatus.Paid) => true,
                (OrderStatus.Paid, OrderStatus.Shipped) => true,
                //(OrderStatus.Paid, OrderStatus.Cancelled) => true,
                (OrderStatus.New, OrderStatus.Cancelled) => true,
                _ => false
            };
        }
    }
}
