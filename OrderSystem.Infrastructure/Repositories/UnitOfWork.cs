using Microsoft.EntityFrameworkCore.Storage;
using OrderSystem.Infrastructure.Context;
using OrderSystem.Application.Interfaces.Repositories;

namespace OrderSystem.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private readonly OrderContext _orderContext;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(OrderContext context)
        {
            _orderContext = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _orderContext.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _orderContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction.");

            await _orderContext.SaveChangesAsync();
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction.");

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();
        }
    }
}
