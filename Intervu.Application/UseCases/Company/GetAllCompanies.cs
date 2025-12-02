using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Company;
using Intervu.Application.Interfaces.UseCases.Company;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.Company
{
    public class GetAllCompanies : IGetAllCompanies
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        public GetAllCompanies(ICompanyRepository companyRepository, IMapper mapper)
        {
            _companyRepository = companyRepository;
            _mapper = mapper;
        }
        public async Task<PagedResult<CompanyDto>> ExecuteAsync(int page, int pageSize)
        {
            var (items, total) = await _companyRepository.GetPagedCompaniesAsync(page, pageSize);
            return new PagedResult<CompanyDto>
            (
                _mapper.Map<List<CompanyDto>>(items),
                total,
                pageSize,
                page
            );
        }
    }
}
