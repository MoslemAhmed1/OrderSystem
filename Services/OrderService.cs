using AutoMapper;
using OrderSystem.DTOs.Orders;
using OrderSystem.Mappings;
using OrderSystem.Models;
using OrderSystem.Repositories;
using OrderSystem.Services.Discount;

namespace OrderSystem.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;
        private readonly IDiscountPolicy _discountPolicy;
        //private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, ICustomerRepository customerRepository, IProductRepository productRepository, IUnitOfWork uow, IDiscountPolicy discountPolicy /*, IMapper mapper*/)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _productRepository = productRepository;
            _uow = uow;
            _discountPolicy = discountPolicy;
            //_mapper = mapper;
        }
        
        public async Task<OrderResponse> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);

            if(order is null)
                throw new KeyNotFoundException($"Order {id} not found.");

            return order.ToDto();
            //return _mapper.Map<OrderResponse>(order);
        }
        
        public async Task<List<OrderResponse>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            
            return orders.Select(order => order.ToDto()).ToList();
            //return _mapper.Map<List<OrderResponse>>(orders);
        }
        
        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer is null)
                throw new KeyNotFoundException($"Customer {request.CustomerId} not found.");

            var items = await BuildItems(request.Items);
            var order = new Order
            {
                CustomerId = customer.Id,
                Customer = customer, // TODO: this or refetch from databse for customer to be populated in OrderResponse DTO?
                Status = OrderStatus.New,
                Items = items
            };
            
            order.Total = CalculateTotal(items, customer.CustomerType);

            await _orderRepository.AddAsync(order);
            await _uow.CommitAsync();

            //var savedOrder = await _uow.Orders.GetByIdAsync(order.Id); // TODO: this or populate customer & product above ?
            
            return order.ToDto();
            //return _mapper.Map<OrderResponse>(order);
        }

        public async Task<OrderResponse> UpdateStatusAsync(int id, OrderStatus newStatus)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException($"Order {id} not found.");

            if (!IsValidTransition(order.Status, newStatus))
                throw new InvalidOperationException($"Cannot transition order from {order.Status} to {newStatus}.");

            order.Status = newStatus;
            await _uow.CommitAsync();

            return order.ToDto();
            //return _mapper.Map<OrderResponse>(order);
        }

        public async Task<OrderResponse> UpdateItemsAsync(int id, List<CreateOrderItemRequest> newItems)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException($"Order {id} not found.");

            if (order.Status != OrderStatus.New)
                throw new InvalidOperationException("Only orders with status 'New' can have their items updated.");

            await RestockItems(order.Items);

            var items = await BuildItems(newItems);
            order.Items.Clear();
            foreach (var item in items)
                order.Items.Add(item);

            order.Total = CalculateTotal(items, order.Customer.CustomerType);

            await _uow.CommitAsync();

            return order.ToDto();
            //return _mapper.Map<OrderResponse>(order);
        }

        public async Task CancelOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException($"Order {id} not found.");
            
            if (!IsValidTransition(order.Status, OrderStatus.Cancelled))
                throw new InvalidOperationException($"Cannot cancel an order with status '{order.Status}'.");
            
            order.Status = OrderStatus.Cancelled;
            await RestockItems(order.Items);

            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order is null)
                throw new KeyNotFoundException($"Order {id} not found.");

            await RestockItems(order.Items);

            await _orderRepository.DeleteAsync(id);
            await _uow.CommitAsync();
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
                    throw new KeyNotFoundException($"Product {item.ProductId} not found.");
                
                if(product.StockQuantity < item.Qty)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Requested: {item.Qty}, Available: {product.StockQuantity}.");

                product.StockQuantity -= item.Qty;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Product = product, // TODO: this or refetch from databse for product to be populated in OrderResponse DTO?
                    Qty = item.Qty,
                    UnitPrice = product.Price
                };
                orderItems.Add(orderItem);
            }
            
            return orderItems;
       }

        private async Task RestockItems(IEnumerable<OrderItem> items)
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
                (OrderStatus.Paid, OrderStatus.Cancelled) => true,
                (OrderStatus.New, OrderStatus.Cancelled) => true,
                _ => false
            };
        }
    }
}
