using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.Interfaces.UseCases.InterviewType;
using Intervu.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewType
{
    public class GetInterviewType : IGetInterviewType
    {
        private readonly IInterviewTypeRepository _repo;
        private readonly IMapper _mapper;

        public GetInterviewType(IInterviewTypeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<InterviewTypeDto> ExecuteAsync(Guid typeId)
        {
            var it = await _repo.GetByIdAsync(typeId);
            var dto = _mapper.Map<InterviewTypeDto>(it);
            return dto;
        }

        public async Task<PagedResult<InterviewTypeDto>> ExecuteAsync(int pageSize, int currentPage)
        {
            var it = await _repo.GetList(currentPage, pageSize);

            var dto = _mapper.Map<IEnumerable<InterviewTypeDto>>(it);

            int totalItem = it.Count();

            return new PagedResult<InterviewTypeDto>(dto.ToList(), totalItem, pageSize, currentPage);
        }
    }
}
