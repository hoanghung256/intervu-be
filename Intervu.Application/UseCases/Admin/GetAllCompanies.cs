using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Admin;
using Intervu.Domain.Repositories;
using Intervu.Application.Interfaces.UseCases.Admin;

namespace Intervu.Application.UseCases.Admin
{
    public class GetAllCompanies : IGetAllCompaniesForAdmin
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
            var pagedCompanies = await _companyRepository.GetPagedCompaniesAsync(page, pageSize);

            var companyDtos = _mapper.Map<List<CompanyDto>>(pagedCompanies.Items);

            return new PagedResult<CompanyDto>(companyDtos, pagedCompanies.TotalCount, pageSize, page);
        }
    }
}
