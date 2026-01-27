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
            var interviewTypeToUpdate = await _repo.GetByIdAsync(id);
            
            if (interviewTypeToUpdate is null)
            {
                throw new KeyNotFoundException($"InterviewType with ID {id} not found.");
            }
            
            _mapper.Map(interviewTypeDto, interviewTypeToUpdate);
            
            await _repo.SaveChangesAsync();
        }
    }
}
