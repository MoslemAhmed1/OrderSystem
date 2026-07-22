using OrderSystem.Data;

namespace OrderSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderContext _orderContext;
        public IOrderRepository Orders { get; }
        public ICustomerRepository Customers { get; }
        public IProductRepository Products { get; }

        public UnitOfWork(OrderContext context, IOrderRepository orders, ICustomerRepository customers, IProductRepository products)
        {
            _orderContext = context;
            Orders = orders;
            Customers = customers;
            Products = products;
        }
        
        public async Task<int> CommitAsync()
        {
            return await _orderContext.SaveChangesAsync();
        }
    }
}
