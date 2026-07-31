using OrderSystem.Data;

namespace OrderSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly OrderContext _orderContext;

        public UnitOfWork(OrderContext context)
        {
            _orderContext = context;
        }
        
        public async Task<int> CommitAsync()
        {
            return await _orderContext.SaveChangesAsync();
        }
    }
}
