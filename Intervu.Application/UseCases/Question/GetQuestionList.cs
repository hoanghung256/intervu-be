using AutoMapper;
using Intervu.Application.DTOs.Common;
using Intervu.Application.DTOs.Question;
using Intervu.Application.Interfaces.UseCases.Question;
using Intervu.Domain.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace Intervu.Application.UseCases.Question
{
    public class GetQuestionList(IQuestionRepository repository, IMapper mapper)
        : IGetQuestionList
    {
        public async Task<PagedResult<QuestionListItemDto>> ExecuteAsync(QuestionFilterRequest filter)
        {
            if (filter.Page < 1) filter.Page = 1;
            if (filter.PageSize < 1) filter.PageSize = 10;

            var (items, total) = await repository.GetPagedAsync(
                filter.SearchTerm,
                filter.QuestionType,
                filter.Role,
                filter.Level,
                filter.Page,
                filter.PageSize);

            var dtos = items.Select(q =>
            {
                var dto = mapper.Map<QuestionListItemDto>(q);
                dto.CommentCount = q.Comments?.Count ?? 0;
                return dto;
            }).ToList();

            return new PagedResult<QuestionListItemDto>(dtos, total, filter.PageSize, filter.Page);
        }
    }
}
