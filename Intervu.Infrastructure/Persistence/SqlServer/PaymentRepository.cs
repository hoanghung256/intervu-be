using Intervu.Domain.Repositories;
using Intervu.Domain.Entities;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    //public class PaymentRepository : RepositoryBase<Payment>, IPaymentRepository
    //{
    //    public PaymentRepository(IntervuDbContext context) : base(context)
    //    {
    //    }

    //    public async Task<(IReadOnlyList<Payment> Items, int TotalCount)> GetPagedPaymentsAsync(int page, int pageSize)
    //    {
    //        var query = _context.Payments.AsQueryable();

    //        var totalItems = await query.CountAsync();

    //        var items = await query
    //            .OrderByDescending(p => p.TransactionDate)
    //            .Skip((page - 1) * pageSize)
    //            .Take(pageSize)
    //            .ToListAsync();

    //        return (items, totalItems);
    //    }

    //    public async Task<decimal> GetTotalRevenueAsync()
    //    {
    //        return await _context.Payments
    //            .Where(p => p.Status == Domain.Entities.Constants.PaymentStatus.Completed)
    //            .SumAsync(p => p.Amount);
    //    }

    //    public async Task<int> GetTotalPaymentsCountAsync()
    //    {
    //        return await _context.Payments.CountAsync();
    //    }
    //}
}
