using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class TransactionRepository(IntervuPostgreDbContext context) : RepositoryBase<InterviewBookingTransaction>(context), ITransactionRepository
    {
        public async Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id)
        {
            return await _context.InterviewBookingTransaction.FirstOrDefaultAsync(t => t.InterviewerAvailabilityId == id);
        }
    }
}
