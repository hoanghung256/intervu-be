using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using Intervu.Infrastructure.Persistence.SqlServer.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Intervu.Infrastructure.Persistence.SqlServer
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IntervuDbContext _context;
        public CompanyRepository(IntervuDbContext context)
        {
            _context = context;
        }
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

        public async Task<int> GetTotalCompaniesCountAsync()
        {
            return await _context.Companies.CountAsync();
        }
    }
}
