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
    public class UpdateInterviewType : IUpdateInterviewType
    {
        private readonly IInterviewTypeRepository _repo;
        private readonly IMapper _mapper;

        public UpdateInterviewType(IInterviewTypeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task ExecuteAsync(Guid id, InterviewTypeDto interviewTypeDto)
        {
            if (id == Guid.Empty)
                throw new BadRequestException("Type ID must be a valid GUID");

            if (interviewTypeDto.SuggestedDurationMinutes % 30 != 0)
            {
                throw new BadRequestException("Suggested duration must be a multiple of 30 minutes.");
            }

            if (interviewTypeDto.MinPrice > interviewTypeDto.MaxPrice)
            {
                throw new BadRequestException("MinPrice cannot be greater than MaxPrice.");
            }

            var interviewTypeToUpdate = await _repo.GetByIdAsync(id);
            
            if (interviewTypeToUpdate is null)
            {
                throw new NotFoundException($"InterviewType with ID {id} not found.");
            }

            var existingType = await _repo.GetByNameAsync(interviewTypeDto.Name);
            if (existingType != null && existingType.Id != id)
            {
                throw new ConflictException("Interview type name already exists.");
            }
            
            _mapper.Map(interviewTypeDto, interviewTypeToUpdate);
            
            await _repo.SaveChangesAsync();
        }
    }
}
