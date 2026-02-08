using Intervu.Domain.Abstractions.Entity.Interfaces;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IntervuPostgreDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(IntervuPostgreDbContext context, IServiceProvider serviceProvider)
        {
            _context = context;
            _serviceProvider = serviceProvider;
        }

        public TRepository GetRepository<TRepository>() where TRepository : class
        {
            var repository = _serviceProvider.GetRequiredService<TRepository>();
            return repository;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _currentTransaction ??= await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction is null) return;

            await _currentTransaction.CommitAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction is null) return;

            await _currentTransaction.RollbackAsync();
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }

        public void Dispose() => _context.Dispose();
    }
}
