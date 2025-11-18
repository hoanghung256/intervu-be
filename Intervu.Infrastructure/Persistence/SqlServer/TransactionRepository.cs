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
    public class TransactionRepository : RepositoryBase<Transaction>, ITransactionRepository
    {
        public TransactionRepository(IntervuDbContext context) : base(context)
        {
        }

        public async Task<Transaction?> GetByPayOSOrderCode(int payosOrderCode)
        {
            return await _context.Transactions.FirstOrDefaultAsync(t => t.PayOSOrderCode == payosOrderCode);
        }
    }
}
