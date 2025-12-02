using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Company;
using Intervu.Domain.Entities;

namespace Intervu.Application.Interfaces.UseCases.Company
{
    public interface IGetAllCompanies
    {
        Task<PagedResult<CompanyDto>> ExecuteAsync(int page, int pageSize);
    }
}
