using Intervu.Application.DTOs.Common;
using Intervu.Application.Interfaces.UseCases.Industry;
using Intervu.Domain.Entities;
using Intervu.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Industry
{
    public class GetAllIndustries(IIndustryRepository industryRepository) : IGetAllIndustries
    {
        private readonly IIndustryRepository _industryRepository = industryRepository;

        public async Task<PagedResult<Intervu.Domain.Entities.Industry>> ExecuteAsync(int page, int pageSize)
        {
            var result = await _industryRepository.GetPagedIndustriesAsync(page, pageSize);
            return new PagedResult<Intervu.Domain.Entities.Industry>(
                result.Items.ToList(),
                result.TotalCount,
                pageSize,
                page
            );
        }
    }
}
