using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.Common;
using Intervu.Application.Interfaces.Repositories;
using Intervu.Domain.Entities;
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
        public async Task<PagedResult<Company>> GetPagedCompaniesAsync(int page, int pageSize)
        {
            var query = _context.Companies.AsQueryable();

            var totalItems = query.Count();
            
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Company>(items, totalItems, pageSize, page);
        }

        public async Task<int> GetTotalCompaniesCountAsync()
        {
            return await _context.Companies.CountAsync();
        }
    }
}
