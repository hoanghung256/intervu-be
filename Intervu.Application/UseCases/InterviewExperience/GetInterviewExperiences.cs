using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.InterviewExperience;
using Intervu.Application.Interfaces.UseCases.InterviewExperience;
using Intervu.Domain.Repositories;

namespace Intervu.Application.UseCases.InterviewExperience
{

    public class GetInterviewExperiences(IInterviewExperienceRepository repository, IMapper mapper)
            : IGetInterviewExperiences
    {
        public async Task<PagedResult<InterviewExperienceSummaryDto>> ExecuteAsync(InterviewExperienceFilterRequest filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            var (items, total) = await repository.GetPagedAsync(
                filter.SearchTerm, filter.CompanyId, filter.Role, filter.Level,
                filter.LastRoundCompleted, filter.SortBy, filter.Page, filter.PageSize);

            var dtos = items.Select(e =>
            {
                var dto = mapper.Map<InterviewExperienceSummaryDto>(e);
                dto.QuestionCount = e.Questions?.Count ?? 0;
                return dto;
            }).ToList();

            return new PagedResult<InterviewExperienceSummaryDto>(dtos, total, filter.PageSize, filter.Page);
        }
    }
}