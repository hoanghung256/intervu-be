using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Domain.Entities;

namespace Intervu.Domain.Repositories
{
    public interface ICompanyRepository
    {
        Task<(IReadOnlyList<Company> Items, int TotalCount)> GetPagedCompaniesAsync(int page, int pageSize);

        Task<int> GetTotalCompaniesCountAsync();
    }
}
