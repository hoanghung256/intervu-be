using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.PostgreSQL.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.PostgreSQL
{
    public class CompanyRepository(IntervuPostgreDbContext context) : ICompanyRepository
    {
        private readonly IntervuPostgreDbContext _context = context;

        public async Task<(IReadOnlyList<Company> Items, int TotalCount)> GetPagedCompaniesAsync(int page, int pageSize)
        {
            var query = _context.Companies.AsQueryable();

            var totalItems = await query.CountAsync();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalItems);
        }

        public Task<int> GetTotalCompaniesCountAsync()
        {
            throw new NotImplementedException();
        }
    }
}
