using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class TransactionRepository : RepositoryBase<InterviewBookingTransaction>, ITransactionRepository
    {
        public TransactionRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task<InterviewBookingTransaction?> GetByAvailabilityId(int id)
        {
            return await _context.InterviewBookingTransaction.FirstOrDefaultAsync(t => t.InterviewerAvailabilityId == id);
        }
    }
}
