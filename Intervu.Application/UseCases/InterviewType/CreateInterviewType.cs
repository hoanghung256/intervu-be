using AutoMapper;
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
    public class CreateInterviewType : ICreateInterviewType
    {
        private readonly IInterviewTypeRepository _repo;
        private readonly IMapper _mapper;

        public CreateInterviewType(IInterviewTypeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task ExecuteAsync(InterviewTypeDto interviewTypeDto)
        {
            Domain.Entities.InterviewType interviewType = _mapper.Map<Domain.Entities.InterviewType>(interviewTypeDto);
            await _repo.AddAsync(interviewType);
            await _repo.SaveChangesAsync();
        }
    }
}
