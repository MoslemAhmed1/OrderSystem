using OrderSystem.Application.Discount;
using OrderSystem.Application.DTOs.Orders;
using OrderSystem.Application.Interfaces.Repositories;
using OrderSystem.Application.Interfaces.Services;
using OrderSystem.Application.Mappings;
using OrderSystem.Domain.Entities;
using OrderSystem.Domain.Enums;

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
        private readonly ICacheService _cache;

        public OrderService(
            IOrderRepository orderRepository,
            ICustomerRepository customerRepository,
            IProductRepository productRepository,
            IUnitOfWork uow,
            IDiscountPolicy discountPolicy,
            ITranslationService translation,
            ICacheService cache)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _uow = uow;
            _discountPolicy = discountPolicy;
            _translation = translation;
            _cache = cache;
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
        
        public async Task<List<OrderResponse>> GetAllAsync(int userId, bool isAdmin)
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
                    throw new KeyNotFoundException(_translation.Translate("CustomerNotFound", userId));

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

            var (items, affectedProductIds) = await BuildItems(request.Items);
            var order = new Order
            {
                CustomerId = customer.Id,
                Status = OrderStatus.New,
                Items = items
            };
            
            order.Total = CalculateTotal(items, customer.CustomerType);

            await _orderRepository.AddAsync(order);
            await _uow.CommitAsync();

            await InvalidateProductCacheAsync(affectedProductIds);
            
            var savedOrder = await _orderRepository.GetByIdAsync(order.Id);
            if (savedOrder is null)
                throw new InvalidOperationException(_translation.Translate("OrderCreationFailed", order.Id));
            
            return savedOrder.ToDto();
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
            await _uow.CommitAsync();

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

            var oldProductIds = await RestockItems(order.Items);

            var (items, newProductIds) = await BuildItems(newItems);
            order.Items.Clear();
            foreach (var item in items)
                order.Items.Add(item);

            order.Total = CalculateTotal(items, order.Customer.CustomerType);

            await _uow.CommitAsync();

            var allAffectedIds = oldProductIds.Union(newProductIds).ToList();
            await InvalidateProductCacheAsync(allAffectedIds);

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
            var affectedProductIds = await RestockItems(order.Items);

            await _uow.CommitAsync();

            await InvalidateProductCacheAsync(affectedProductIds);
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException(_translation.Translate("OrderNotFound", id));

            if (order.Status != OrderStatus.Cancelled)
                throw new InvalidOperationException("Only cancelled orders can be soft deleted.");

            order.IsDeleted = true;
            await _uow.CommitAsync();
        }

        private async Task<(List<OrderItem> Items, List<int> AffectedProductIds)> BuildItems(List<CreateOrderItemRequest> items)
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
            
            return (orderItems, productIds);
        }

        private async Task<List<int>> RestockItems(ICollection<OrderItem> items)
        {
            var productIds = items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _productRepository.GetByIdsAsync(productIds);

            foreach (var item in items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product is not null)
                    product.StockQuantity += item.Qty;
            }

            return productIds;
        }

        private async Task InvalidateProductCacheAsync(List<int> productIds)
        {
            foreach (var id in productIds)
                await _cache.RemoveAsync($"product_{id}");

            await _cache.RemoveAsync("products_all");
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
