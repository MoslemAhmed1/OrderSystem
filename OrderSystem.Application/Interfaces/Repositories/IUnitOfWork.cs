namespace OrderSystem.Application.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
    }
}
