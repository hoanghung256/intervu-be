using AutoMapper;
using Intervu.Application.DTOs.InterviewType;
using Intervu.Application.Exceptions;
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
            if (interviewTypeDto.SuggestedDurationMinutes % 30 != 0)
            {
                throw new BadRequestException("Suggested duration must be a multiple of 30 minutes.");
            }

            if (interviewTypeDto.MinPrice > interviewTypeDto.MaxPrice)
            {
                throw new BadRequestException("MinPrice cannot be greater than MaxPrice.");
            }

            var existingType = await _repo.GetByNameAsync(interviewTypeDto.Name);
            if (existingType != null)
            {
                throw new ConflictException("Interview type name already exists.");
            }

            Domain.Entities.InterviewType interviewType = _mapper.Map<Domain.Entities.InterviewType>(interviewTypeDto);
            await _repo.AddAsync(interviewType);
            await _repo.SaveChangesAsync();
        }
    }
}
