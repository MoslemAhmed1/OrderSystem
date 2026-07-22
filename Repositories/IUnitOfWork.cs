namespace OrderSystem.Repositories
{
    public interface IUnitOfWork
    {
        IOrderRepository Orders { get; }
        ICustomerRepository Customers { get; }
        IProductRepository Products { get; }
        Task<int> CommitAsync();
    }
}
