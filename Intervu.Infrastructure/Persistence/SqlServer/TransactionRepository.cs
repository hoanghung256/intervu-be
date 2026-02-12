using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;
using Intervu.Domain.Entities.Constants;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class TransactionRepository : RepositoryBase<InterviewBookingTransaction>, ITransactionRepository
    {
        public TransactionRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task<InterviewBookingTransaction?> Get(int orderCode, TransactionType type)
        {
            return await _context.InterviewBookingTransaction
                .FirstOrDefaultAsync(t => t.OrderCode == orderCode && t.Type == type);
        }

        public async Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id)
        {
            return await _context.InterviewBookingTransaction.FirstOrDefaultAsync(t => t.CoachAvailabilityId == id);
        }

        public async Task<InterviewBookingTransaction?> GetByAvailabilityId(Guid id, TransactionType type)
        {
            return await _context.InterviewBookingTransaction
                .FirstOrDefaultAsync(t => t.CoachAvailabilityId == id && t.Type == type);
        }
    }
}
