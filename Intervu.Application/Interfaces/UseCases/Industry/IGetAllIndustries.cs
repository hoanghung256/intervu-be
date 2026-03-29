using Intervu.Application.DTOs.Common;
using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Interfaces.UseCases.Industry
{
    public interface IGetAllIndustries
    {
        Task<PagedResult<Intervu.Domain.Entities.Industry>> ExecuteAsync(int page, int pageSize);
    }
}
