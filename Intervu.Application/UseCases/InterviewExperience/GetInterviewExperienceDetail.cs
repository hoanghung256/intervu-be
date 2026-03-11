using AutoMapper;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.Interfaces.UseCases.InterviewExperience;
using Intervu.Domain.Repositories;
using System;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.InterviewExperience
{
    public class GetInterviewExperienceDetail(IInterviewExperienceRepository repository, IMapper mapper)
        : IGetInterviewExperienceDetail
    {
        public async Task<InterviewExperienceDetailDto?> ExecuteAsync(Guid id)
        {
            var entity = await repository.GetDetailAsync(id);
            if (entity == null) return null;
            var dto = mapper.Map<InterviewExperienceDetailDto>(entity);
            dto.QuestionCount = entity.Questions?.Count ?? 0;
            return dto;
        }
    }
}
