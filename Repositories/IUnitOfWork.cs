namespace OrderSystem.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
    }
}
